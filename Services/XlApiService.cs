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

        #region WarehouseDocuments
        public string CreateWarehouseDocument(CreateWarehouseDocumentDTO document)
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
                Magazyn = document.Warehouse.ToString(),
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
                int productResult = AddProductToWarehouseDocument(documentId, product);
                if (productResult != 0)
                {
                    errorMessage = CheckError((int)ErrorCode.DodajPozycjeMag, productResult);
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }
                _logger.LogInformation($"Added product {product.ProductCode} to document {document.WmsName}.");
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

            int closeResult = CloseWarehouseDocument(documentId, document.ErpType, document.Status);
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
            document.ErpId = xLDokumentMag.GIDNumer;
            return errorMessage;
        }

        public async Task<string> EditWarehouseDocument(EditWarehouseDocumentDTO document)
        {
            AttachThreadToClarion(1);

            int documentId = 0;
            string errorMessage = String.Empty;

            if (!string.IsNullOrEmpty(document.Status.ToString()))
            {
                ManageTransaction(0); // Otwieramy transakcje

                int openDocResult = OpenWarehouseDocument(document.ErpId, document.ErpType, out documentId);
                if (openDocResult != 0)
                {
                    errorMessage = CheckError((int)ErrorCode.OtworzDokumentMag, openDocResult);
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }

                if (document.PositionsToRealize?.Count > 0)
                {
                    foreach (var product in document.PositionsToRealize)
                    {
                        int addRealizationResult = AddProductRealizationToWareHouseDocument(documentId, product);
                        if (addRealizationResult != 0)
                        {
                            errorMessage = CheckError((int)ErrorCode.RealizujPozycjeMag, addRealizationResult);
                            ManageTransaction(2); // Zamykamy transakcje
                            return errorMessage;
                        }
                    }
                }

                if (document.PositionsToAdd?.Count > 0)
                {
                    foreach (var position in document.PositionsToAdd)
                    {
                        int addProductResult = AddProductToWarehouseDocument(documentId, position);
                        if (addProductResult != 0)
                        {
                            errorMessage = CheckError((int)ErrorCode.DodajPozycjeMag, addProductResult);
                            ManageTransaction(2); // Zamykamy transakcje
                            return errorMessage;
                        }
                    }
                }

                int closeDocResult = CloseWarehouseDocument(documentId, document.ErpType, document.Status);
                if (closeDocResult != 0)
                {
                    errorMessage = CheckError((int)ErrorCode.ZamknijDokumentMag, closeDocResult);
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }

                ManageTransaction(1); // Potwierdzamy transakcje
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

            return errorMessage;

        }

        private int AddProductToWarehouseDocument(int documentId, DocumentProductDTO product)
        {
            XLDokumentMagElemInfo_20241 xLDokumentMagElem = new()
            {
                Wersja = _config.ApiVersion,
                Ilosc = product.Quantity,
                TowarKod = product.ProductCode,
                JmZ = product.Unit
            };

            int result = cdn_api.cdn_api.XLDodajPozycjeMag(documentId, xLDokumentMagElem);
            product.ProductLp = xLDokumentMagElem.GIDLp;
            return result;
        }

        private int AddProductRealizationToWareHouseDocument(int documentId, PositionsToRealizeDTO product)
        {
            XLRealizujPozycjeMagInfo_20241 xLRealizujPozycje = new XLRealizujPozycjeMagInfo_20241()
            {
                Wersja = _config.ApiVersion,
                EleLp = product.ProductLp,
                Ilosc = product.Quantity,
            };

            int result = cdn_api.cdn_api.XLRealizujPozycjeMag(documentId, xLRealizujPozycje);
            return result;
        }

        private int OpenWarehouseDocument(int documentErpId, WarehouseDocumentType documentType, out int documentId)
        {
            int result = -1;
            documentId = 0;

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

            return result;
        }

        private int CloseWarehouseDocument(int documentId, WarehouseDocumentType type, DocumentStatus? status = null)
        {
            int result = -1;
            XLZamkniecieDokumentuMagInfo_20241 xLZamkniecieDokumentuMag = new();
            xLZamkniecieDokumentuMag.Wersja = _config.ApiVersion;

            if (status is not null)
            {
                xLZamkniecieDokumentuMag.Tryb = (int)status;
            }

            result = cdn_api.cdn_api.XLZamknijDokumentMag(documentId, xLZamkniecieDokumentuMag);

            return result;
        }
        #endregion
        #region TradingDocuments
        public string CreateTradingDocument(CreateTradingDocumentDTO document)
        {
            AttachThreadToClarion(1);
            ManageTransaction(0); // Otwieramy transakcje

            string errorMessage = string.Empty;
            int documentId = 0;
            XLDokumentNagInfo_20241 xLDokumentHand = new()
            {
                Wersja = _config.ApiVersion,
                Typ = (int)document.ErpType,
                MagazynZ = document.SourceWarehouse.ToString(),
                MagazynD = document.DestinationWarehouse.ToString(),
                Opis = document.Description,
                Cecha = document.WmsName,
                ZwrNumer = document.SourceDocId,
                ZwrTyp = (int)document.SourceDocType,
                ZwrLp = 0,
                ZwrFirma = 449892
            };

            int createResult = cdn_api.cdn_api.XLNowyDokument(_sessionId, ref documentId, xLDokumentHand);

            if (createResult != 0 || documentId == 0)
            {
                errorMessage = CheckError((int)ErrorCode.NowyDokument, createResult);
                ManageTransaction(2); // Zamykamy transakcje
                return errorMessage;
            }
            foreach (var product in document.Products)
            {
                int productResult = AddProductToTradingDocument(documentId, product);
                if (productResult != 0)
                {
                    errorMessage = CheckError((int)ErrorCode.DodajPozycje, productResult);
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }
                _logger.LogInformation($"Added product {product.ProductCode} to document {document.WmsName}.");
            }

            if (document.Attributes?.Count >= 1)
            {
                foreach (var attribute in document.Attributes)
                {
                    int addAttributeResult = AddAttribute(xLDokumentHand.GIDNumer, (int)document.ErpType, 0, attribute);
                    if (addAttributeResult != 0)
                    {
                        errorMessage = $"Error when adding attribute {attribute.Name} with value {attribute.Value} to document {document.WmsName}";
                        ManageTransaction(2); // Zamykamy transakcje
                        return errorMessage;
                    }
                }
            }

            int closeResult = CloseTradingDocument(documentId, document.ErpType, document.Status);
            if (closeResult != 0)
            {
                errorMessage = CheckError((int)ErrorCode.ZamknijDokumentMag, closeResult);
                ManageTransaction(2); // Zamykamy transakcje
                return errorMessage;
            }

            _logger.LogInformation($"Added document with WMSName: {document.WmsName} ({document.WmsId})");
            ManageTransaction(1); // Potwierdzamy transakcje
            document.ErpId = xLDokumentHand.GIDNumer;
            return errorMessage;
        }

        public async Task<string> EditTradingDocument(EditTradingDocumentDTO document)
        {
            AttachThreadToClarion(1);
            int documentId = 0;
            string errorMessage = String.Empty;

            if (!string.IsNullOrEmpty(document.Status.ToString()))
            {
                ManageTransaction(0); // Otwieramy transakcje
                int openDocResult = OpenTradingDocument(document.ErpId, document.ErpType, out documentId);
                if (openDocResult != 0)
                {
                    errorMessage = CheckError((int)ErrorCode.OtworzDokumentHan, openDocResult);
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }

                int closeDocResult = CloseTradingDocument(documentId, document.ErpType, document.Status);
                if (closeDocResult != 0)
                {
                    errorMessage = CheckError((int)ErrorCode.ZamknijDokument, closeDocResult);
                    ManageTransaction(2); // Zamykamy transakcje
                    return errorMessage;
                }
                ManageTransaction(1); // Potwierdzamy transakcje
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

            return errorMessage;

        }

        private int AddProductToTradingDocument(int documentId, DocumentProductDTO product)
        {
            XLDokumentElemInfo_20241 xLDokumentElem = new()
            {
                Wersja = _config.ApiVersion,
                Ilosc = product.Quantity,
                TowarKod = product.ProductCode,
                JmZ = product.Unit
            };

            int result = cdn_api.cdn_api.XLDodajPozycje(documentId, xLDokumentElem);
            return result;
        }

        private int OpenTradingDocument(int documentErpId, TradingDocumentType documentType, out int documentId)
        {
            int result = -1;
            documentId = 0;

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

            return result;
        }

        private int CloseTradingDocument(int documentId, TradingDocumentType type, DocumentStatus? status)
        {
            int result = -1;

            XLZamkniecieDokumentuInfo_20241 xLZamkniecieDokumentu = new()
            {
                Wersja = _config.ApiVersion,
                Tryb = (int)status
            };
            result = cdn_api.cdn_api.XLZamknijDokument(documentId, xLZamkniecieDokumentu);

            return result;
        }
        #endregion
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

        private int ConnectDocuments(int documentId1, WarehouseDocumentType documentType1, int documentId2, TradingDocumentType documentType2, int connectionType)
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
