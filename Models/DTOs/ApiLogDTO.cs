using APIWMS.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models.DTOs
{
    public class ApiLogDTO
    {
        public int EntityWmsId { get; set; }
        public DocumentType EntityWmsType { get; set; }
        public int EntityErpId { get; set; }
        public DocumentType EntityErpType { get; set; }
        public string Action { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
