using APIWMS.Data.Enums;
using APIWMS.Models.DTOs;
using APIWMS.Models.ViewModels;

namespace APIWMS.Interfaces
{
    public interface IXlApiService
    {
        public int Login();
        public int Logout();
        public string CreateTradingDocument(CreateTradingDocumentDTO createTradingDocumentDTO);
        public Task<string> EditTradingDocument(EditTradingDocumentDTO editTradingDocumentDTO);
        public string CreateWarehouseDocument(CreateWarehouseDocumentDTO createWarehouseDocumentDTO);
        public Task<string> EditWarehouseDocument(EditWarehouseDocumentDTO editWarehouseDocumentDTO);
        public int AddAttribute(int obiNumer, int obiType, int obiLp, Models.Attribute attribute);
    }
}
