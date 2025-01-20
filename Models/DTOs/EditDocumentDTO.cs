using APIWMS.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace APIWMS.Models.DTOs
{
    public class EditDocumentDTO
    {
        [Required]
        public required int WmsId { get; set; }
        [Required]
        public required int WmsType { get; set; }
        [Required]
        public required int ErpId { get; set; }
        [Required]
        public required DocumentType ErpType { get; set; }
        public string? Status { get; set; }
        public List<Attribute>? Attributes { get; set; }
    }
}
