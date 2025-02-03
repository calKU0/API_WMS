using APIWMS.Data.Enums;
using APIWMS.Models.DTOs;
using APIWMS.Models.ViewModels;

namespace APIWMS.Interfaces
{
    public interface IXlApiService
    {
        public int Login();
        public int Logout();
        public string CreateDocument(CreateDocumentDTO createDocumentDTO);
        public Task<string> ModifyDocument(EditDocumentDTO editDocumentDTO);
        public int AddAttribute(int obiNumer, int obiType, int obiLp, Models.Attribute attribute);
    }
}
