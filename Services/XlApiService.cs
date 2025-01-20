using APIWMS.Data.Enums;
using APIWMS.Helpers;
using APIWMS.Interfaces;
using APIWMS.Models.DTOs;
using APIWMS.Models.ViewModels;
using cdn_api;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace APIWMS.Services
{
    public class XlApiService : IXlApiService
    {
        [DllImport("ClaRUN.dll")]
        public static extern void AttachThreadToClarion(int _flag);
        private readonly IOptions<XlApiSettings> _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IXlApiService> _logger;
        private int _sessionId;
        public XlApiService(IOptions<XlApiSettings> config, ILogger<IXlApiService> logger, IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public int Login()
        {
            XLLoginInfo_20241 xLLoginInfo = new()
            {
                Wersja = _config.Value.ApiVersion,
                ProgramID = _config.Value.ProgramName,
                Baza = _config.Value.Database,
                OpeIdent = _config.Value.Username,
                OpeHaslo = _config.Value.Password
            };

            int result = cdn_api.cdn_api.XLLogin(xLLoginInfo, ref _sessionId);
            return result;
        }

        public int Logout()
        {
            AttachThreadToClarion(1);
            XLLogoutInfo_20241 xLLogoutInfo = new()
            {
                Wersja = _config.Value.ApiVersion,
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
                Wersja = _config.Value.ApiVersion,
                Typ = (int)document.ErpType,
                Akronim = document.Client,
                Magazyn = document.Wearhouse,
                Opis = document.Description,
            };

            int createResult = cdn_api.cdn_api.XLNowyDokumentMag(_sessionId, ref documentId, xLDokumentMag);

            if (createResult != 0 || documentId == 0)
            {
                errorMessage = CheckError((int)ErrorCode.NowyDokumentMag, createResult);
                _logger.LogError($"Błąd przy tworzeniu dokumentu: {errorMessage}");
                ManageTransaction(2); // Zamykamy transakcje
                return errorMessage;
            }
            foreach (var product in document.Products) 
            { 
                int productResult = AddProductToDocument(documentId, product);
                if (productResult != 0) 
                {
                    errorMessage = CheckError((int)ErrorCode.DodajPozycjeMag, productResult);
                    _logger.LogError($"Błąd dodawaniu produktu {product.Code} Błąd: {errorMessage}" );
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }
                _logger.LogInformation($"Dodano produkt {product.Code} do dokumentu");
            }

            if (document.Attributes?.Count >= 1)
            {
                foreach (var attribute in document.Attributes) 
                {
                    int addAttributeResult = AddAttribute(xLDokumentMag.GIDNumer, document.ErpType, attribute);
                    if (addAttributeResult != 0)
                    {
                        errorMessage = addAttributeResult.ToString();
                        _logger.LogError($"Błąd dodawania atrybutu {attribute.Name} o wartości {attribute.Value} do dokumentu {documentId}. Błąd: {errorMessage}");
                        ManageTransaction(2); // Zamykamy transakcje
                        return errorMessage;
                    }
                }
            }

            int closeResult = CloseDocument(documentId, document.ErpType, document.Status);
            if (closeResult != 0)
            {
                errorMessage = CheckError((int)ErrorCode.ZamknijDokumentMag, closeResult);
                _logger.LogError($"Błąd zamykania dokumentu {errorMessage}");
                ManageTransaction(2); // Zamykamy transakcje
                return errorMessage;
            }

            int connectionResult = ConnectDocuments(xLDokumentMag.GIDNumer, document.ErpType, document.SourceId, document.SourceType, 3);
            if (connectionResult != 0)
            {
                errorMessage = CheckError((int)ErrorCode.ZepnijDokument, connectionResult);
                _logger.LogError($"Błąd podczas spinania dokumentów handlowych z magazynowymi. Błąd: {errorMessage}");
                ManageTransaction(2); // Zamykamy transakcje
                return errorMessage;
            }

            _logger.LogInformation($"Założono dokument WMSID: {document.WmsId}");
            ManageTransaction(1); // Potwierdzamy transakcje
            return errorMessage;
        }

        public string ModifyDocument(EditDocumentDTO document)
        {
            AttachThreadToClarion(1);

            int documentId = 0;
            string errorMessage = String.Empty;
            int openErrorCode;
            int closeErrorCode;

            if (document.Attributes?.Count >= 1)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
                    foreach (var attribute in document.Attributes)
                    {
                        int updateAttributeResult = context.UpdateAttribute(document, document.Attributes);
                        if (updateAttributeResult <= 0)
                        {
                            _logger.LogError($"Błąd aktualizowania atrybutu {attribute.Name} o wartości {attribute.Value} na dokumencie o ERPID: {document.ErpId}");
                            errorMessage = $"Błąd aktualizowania atrybutu {attribute.Name} o wartości {attribute.Value} na dokumencie o ERPID: {document.ErpId}";
                            return errorMessage;
                        }
                        _logger.LogInformation($"Zaktualizowano atrybut '{attribute.Name}' o wartości {attribute.Value} na dokumencie o ERPID: {document.ErpId}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(document.Status))
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
                    _logger.LogError($"Błąd otwierania dokumentu o ERPID {document.ErpId} : {errorMessage}");
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }

                int closeDocResult = CloseDocument(documentId, document.ErpType, document.Status);
                if (closeDocResult != 0)
                {
                    errorMessage = CheckError(closeErrorCode, closeDocResult);
                    _logger.LogError($"Błąd zamykania dokumentu o ERPID: {document.ErpId}: {errorMessage}");
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }
                _logger.LogInformation($"Zaktualizowano status dokumentu o ERPID: {document.ErpId}");
            }
            ManageTransaction(1); // Potwierdzamy transakcje
            return errorMessage;

        }

        public int OpenDocument(int documentErpId, DocumentType documentType, out int documentId)
        {
            int result = -1;
            documentId = 0;

            if (DocumentTypeGroups.DokHandlowy.Contains(documentType))
            {
                XLOtwarcieNagInfo_20241 xLOtwarcieNag = new()
                {
                    Wersja = _config.Value.ApiVersion,
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
                    Wersja = _config.Value.ApiVersion,
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

        public int CloseDocument(int documentId, DocumentType type, string status)
        {
            int result = -1;
            if (DocumentTypeGroups.DokHandlowy.Contains(type))
            {
                XLZamkniecieDokumentuInfo_20241 xLZamkniecieDokumentu = new()
                {
                    Wersja = _config.Value.ApiVersion,
                    Tryb = Convert.ToInt16(status)
                };
                result = cdn_api.cdn_api.XLZamknijDokument(documentId, xLZamkniecieDokumentu);
            }
            else
            {
                XLZamkniecieDokumentuMagInfo_20241 xLZamkniecieDokumentuMag = new()
                {
                    Wersja = _config.Value.ApiVersion,
                    Tryb = Convert.ToInt16(status)
                };
                result = cdn_api.cdn_api.XLZamknijDokumentMag(documentId, xLZamkniecieDokumentuMag);
            }
            return result;
        }

        public int AddAttribute(int obiNumer, DocumentType obiType, Models.Attribute attribute)
        {
            XLAtrybutInfo_20241 xLAtrybut = new()
            {
                Wersja = _config.Value.ApiVersion,
                Klasa = attribute.Name,
                Wartosc = attribute.Value,
                GIDTyp = (int)obiType,
                GIDNumer = obiNumer,
                GIDLp = 0,
                GIDSubLp = 0,
                GIDFirma = 449892
            };

            int result = cdn_api.cdn_api.XLDodajAtrybut(_sessionId, xLAtrybut);
            
            return result;
        }

        public int AddProductToDocument(int documentId, AddProductToDocumentDTO product)
        {
            XLDokumentMagElemInfo_20241 xLDokumentMagElem = new()
            {
                Wersja = _config.Value.ApiVersion,
                Ilosc = product.Quantity,
                TowarKod = product.Code
            };

            int result = cdn_api.cdn_api.XLDodajPozycjeMag(documentId, xLDokumentMagElem);
            return result;
        }

        public string CheckError(int function, int errorCode)
        {
            XLKomunikatInfo_20241 xLKomunikat = new()
            {
                Wersja = _config.Value.ApiVersion,
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

        public int ConnectDocuments(int documentId1, DocumentType documentType1, int documentId2, DocumentType documentType2, int connectionType)
        {
            XLDokSpiDokInfo_20241 xLDokSpi = new()
            {
                Wersja = _config.Value.ApiVersion,
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

        public int ManageTransaction(int type, string token = "")
        {
            XLTransakcjaInfo_20241 xLTransakcja = new()
            {
                Wersja = _config.Value.ApiVersion,
                Tryb = type
            };
            int result = cdn_api.cdn_api.XLTransakcja(_sessionId, xLTransakcja);
            return result;
        }

        public int ModifyProduct(int productId, string field, string value)
        {
            throw new NotImplementedException();
        }
    }
}
