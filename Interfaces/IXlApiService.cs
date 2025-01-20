using APIWMS.Data.Enums;
using APIWMS.Models.DTOs;
using APIWMS.Models.ViewModels;

namespace APIWMS.Interfaces
{
    public interface IXlApiService
    {
        public int Login();
        public int Logout();
        public int ModifyProduct(int productId, string field, string value);
        public string CreateDocument(CreateDocumentDTO createDocumentDTO);
        public int OpenDocument(int documentErpId, DocumentType type, out int documentId);
        public int CloseDocument(int documentId, DocumentType type, string status);
        public string ModifyDocument(EditDocumentDTO editDocumentDTO);
        public int AddProductToDocument(int documentId, AddProductToDocumentDTO product);
        public int AddAttribute(int ObiNumer, DocumentType ObiType, Models.Attribute attribute);
        public string CheckError(int function, int errorCode);
        public int ConnectDocuments(int documentId1, DocumentType documentType1, int documentId2, DocumentType documentType2, int connectionType);
        public int ManageTransaction(int type, string token = "");
    }
}
