using APIWMS.Data.Enums;
using APIWMS.Helpers;
using APIWMS.Interfaces;
using APIWMS.Models;
using APIWMS.Models.DTOs;
using APIWMS.Models.ViewModels;
using cdn_api;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace APIWMS.Services
{
    public class XlApiService : IXlApiService
    {
        [DllImport("ClaRUN.dll")]
        public static extern void AttachThreadToClarion(int _flag);
        private readonly XlApiSettings _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IXlApiService> _logger;
        private int _sessionId;
        public XlApiService(IOptions<XlApiSettings> config, ILogger<IXlApiService> logger, IServiceProvider serviceProvider)
        {
            _config = config.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public int Login()
        {
            XLLoginInfo_20241 xLLoginInfo = new()
            {
                Wersja = _config.ApiVersion,
                ProgramID = _config.ProgramName,
                Baza = _config.Database,
                OpeIdent = _config.Username,
                OpeHaslo = _config.Password,
                TrybWsadowy = 1
            };

            int result = cdn_api.cdn_api.XLLogin(xLLoginInfo, ref _sessionId);
            return result;
        }

        public int Logout()
        {
            AttachThreadToClarion(1);
            XLLogoutInfo_20241 xLLogoutInfo = new()
            {
                Wersja = _config.ApiVersion,
            };

            int result = cdn_api.cdn_api.XLLogout(_sessionId);
            return result;
        }

        public string CreateDocument(CreateDocumentDTO document)
        {
            AttachThreadToClarion(1);
            ManageTransaction(0); // Otwieramy transakcje

            string errorMessage = string.Empty;
            int documentId = 0;
            XLDokumentMagNagInfo_20241 xLDokumentMag = new()
            {
                Wersja = _config.ApiVersion,
                Typ = (int)document.ErpType,
                Akronim = document.ClientName,
                Magazyn = document.Wearhouse,
                Opis = document.Description,
                Cecha = document.WmsName,
            };

            int createResult = cdn_api.cdn_api.XLNowyDokumentMag(_sessionId, ref documentId, xLDokumentMag);

            if (createResult != 0 || documentId == 0)
            {
                errorMessage = CheckError((int)ErrorCode.NowyDokumentMag, createResult);
                ManageTransaction(2); // Zamykamy transakcje
                return errorMessage;
            }
            foreach (var product in document.Products)
            {
                int productResult = AddProductToDocument(documentId, product);
                if (productResult != 0)
                {
                    errorMessage = CheckError((int)ErrorCode.DodajPozycjeMag, productResult);
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }
                _logger.LogInformation($"Added product {product.Code} to document {document.WmsName}.");
            }

            if (document.Attributes?.Count >= 1)
            {
                foreach (var attribute in document.Attributes)
                {
                    int addAttributeResult = AddAttribute(xLDokumentMag.GIDNumer, (int)document.ErpType, 0, attribute);
                    if (addAttributeResult != 0)
                    {
                        errorMessage = $"Error when adding attribute {attribute.Name} with value {attribute.Value} to document {document.WmsName}";
                        ManageTransaction(2); // Zamykamy transakcje
                        return errorMessage;
                    }
                }
            }

            int closeResult = CloseDocument(documentId, document.ErpType, document.Status.ToString());
            if (closeResult != 0)
            {
                errorMessage = CheckError((int)ErrorCode.ZamknijDokumentMag, closeResult);
                ManageTransaction(2); // Zamykamy transakcje
                return errorMessage;
            }

            int connectionResult = ConnectDocuments(xLDokumentMag.GIDNumer, document.ErpType, document.SourceDocId, document.SourceDocType, 3);
            if (connectionResult != 0)
            {
                errorMessage = CheckError((int)ErrorCode.ZepnijDokument, connectionResult);
                ManageTransaction(2); // Zamykamy transakcje
                return errorMessage;
            }

            _logger.LogInformation($"Added document with WMSName: {document.WmsName} ({document.WmsId})");
            ManageTransaction(1); // Potwierdzamy transakcje
            return errorMessage;
        }

        public async Task<string> ModifyDocument(EditDocumentDTO document)
        {
            AttachThreadToClarion(1);

            int documentId = 0;
            string errorMessage = String.Empty;
            int openErrorCode;
            int closeErrorCode;

            if (!string.IsNullOrEmpty(document.Status.ToString()))
            {
                ManageTransaction(0); // Otwieramy transakcje
                if (DocumentTypeGroups.DokHandlowy.Contains((DocumentType)document.ErpType))
                {
                    openErrorCode = (int)ErrorCode.OtworzDokumentHan;
                    closeErrorCode = (int)ErrorCode.ZamknijDokument;
                }
                else
                {
                    openErrorCode = (int)ErrorCode.OtworzDokumentMag;
                    closeErrorCode = (int)ErrorCode.ZamknijDokumentMag;
                }

                int openDocResult = OpenDocument(document.ErpId, document.ErpType, out documentId);
                if (openDocResult != 0)
                {
                    errorMessage = CheckError(openErrorCode, openDocResult);
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }

                int closeDocResult = CloseDocument(documentId, document.ErpType, document.Status.ToString());
                Console.WriteLine(closeDocResult);
                if (closeDocResult != 0)
                {
                    errorMessage = CheckError(closeErrorCode, closeDocResult);
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }
                _logger.LogInformation($"Updated document status ERPID: {document.ErpId}");
            }

            if (document.Attributes?.Count >= 1)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

                    var failedAttributes = await context.UpdateAttributes(document.ErpId, (int)document.ErpType, 0, document.Attributes);

                    if (failedAttributes.Count != 0)
                    {
                        foreach (var failedAttribute in failedAttributes)
                        {
                            errorMessage = $"Error updating attribute {failedAttribute} on document with ERPID: {document.ErpId}";
                            return errorMessage;
                        }
                    }
                    _logger.LogInformation($"Updated attributes on document with ERPID: {document.ErpId}");
                }
            }

            ManageTransaction(1); // Potwierdzamy transakcje
            return errorMessage;

        }

        private int OpenDocument(int documentErpId, DocumentType documentType, out int documentId)
        {
            int result = -1;
            documentId = 0;

            if (DocumentTypeGroups.DokHandlowy.Contains(documentType))
            {
                XLOtwarcieNagInfo_20241 xLOtwarcieNag = new()
                {
                    Wersja = _config.ApiVersion,
                    Tryb = 2,
                    GIDTyp = (int)documentType,
                    GIDNumer = documentErpId,
                    GIDFirma = 449892,
                    GIDLp = 0,
                };
                result = cdn_api.cdn_api.XLOtworzDokument(_sessionId, ref documentId, xLOtwarcieNag);
            }
            if (DocumentTypeGroups.DokMagazynowe.Contains(documentType))
            {
                XLOtwarcieMagNagInfo_20241 xLOtwarcieMagNag = new()
                {
                    Wersja = _config.ApiVersion,
                    Tryb = 2,
                    GIDTyp = (int)documentType,
                    GIDNumer = documentErpId,
                    GIDFirma = 449892,
                    GIDLp = 0,
                };
                result = cdn_api.cdn_api.XLOtworzDokumentMag(_sessionId, ref documentId, xLOtwarcieMagNag);
            }
            return result;
        }

        private int CloseDocument(int documentId, DocumentType type, string status)
        {
            int result = -1;
            if (DocumentTypeGroups.DokHandlowy.Contains(type))
            {
                XLZamkniecieDokumentuInfo_20241 xLZamkniecieDokumentu = new()
                {
                    Wersja = _config.ApiVersion,
                    Tryb = Convert.ToInt32(status)
                };
                result = cdn_api.cdn_api.XLZamknijDokument(documentId, xLZamkniecieDokumentu);
            }
            else
            {
                XLZamkniecieDokumentuMagInfo_20241 xLZamkniecieDokumentuMag = new()
                {
                    Wersja = _config.ApiVersion,
                    Tryb = Convert.ToInt32(status)
                };
                result = cdn_api.cdn_api.XLZamknijDokumentMag(documentId, xLZamkniecieDokumentuMag);
            }
            return result;
        }

        public int AddAttribute(int obiNumer, int obiType, int obiLp, Models.Attribute attribute)
        {
            AttachThreadToClarion(1);
            XLAtrybutInfo_20241 xLAtrybut = new()
            {
                Wersja = _config.ApiVersion,
                Klasa = attribute.Name,
                Wartosc = attribute.Value,
                GIDNumer = obiNumer,
                GIDTyp = obiType,
                GIDLp = obiLp,
                GIDSubLp = 0,
                GIDFirma = 449892,
            };

            int result = cdn_api.cdn_api.XLDodajAtrybut(_sessionId, xLAtrybut);

            return result;
        }

        private int AddProductToDocument(int documentId, DocumentProductDTO product)
        {
            XLDokumentMagElemInfo_20241 xLDokumentMagElem = new()
            {
                Wersja = _config.ApiVersion,
                Ilosc = product.Quantity,
                TowarKod = product.Code
            };

            int result = cdn_api.cdn_api.XLDodajPozycjeMag(documentId, xLDokumentMagElem);
            return result;
        }

        private string CheckError(int function, int errorCode)
        {
            XLKomunikatInfo_20241 xLKomunikat = new()
            {
                Wersja = _config.ApiVersion,
                Funkcja = function,
                Blad = errorCode,
                Tryb = 0
            };
            int result = cdn_api.cdn_api.XLOpisBledu(xLKomunikat);

            if (result == 0)
                return xLKomunikat.OpisBledu;
            else
                return $"Error while checking error. Error code: {result}";
        }

        private int ConnectDocuments(int documentId1, DocumentType documentType1, int documentId2, DocumentType documentType2, int connectionType)
        {
            XLDokSpiDokInfo_20241 xLDokSpi = new()
            {
                Wersja = _config.ApiVersion,
                TypSpi = connectionType,
                GID1Numer = documentId1,
                GID1Typ = (int)documentType1,
                GID1Firma = 449892,
                GID1Lp = 0,
                GID2Numer = documentId2,
                GID2Typ = (int)documentType2,
                GID2Firma = 449892,
                GID2Lp = 0
            };

            int result = cdn_api.cdn_api.XLZepnijDokumenty(xLDokSpi);
            return result;
        }

        private int ManageTransaction(int type, string token = "")
        {
            XLTransakcjaInfo_20241 xLTransakcja = new()
            {
                Wersja = _config.ApiVersion,
                Tryb = type
            };
            int result = cdn_api.cdn_api.XLTransakcja(_sessionId, xLTransakcja);
            return result;
        }
    }
}
