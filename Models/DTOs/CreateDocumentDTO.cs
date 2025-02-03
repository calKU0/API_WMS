using APIWMS.Data.Enums;
using APIWMS.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace APIWMS.Models.ViewModels
{
    public class CreateDocumentDTO
    {
        [Required]
        public required int WmsId { get; set; }
        [Required]
        public required int WmsType { get; set; }
        [Required]
        public required string WmsName { get; set; }
        [Required]
        public required DocumentType ErpType { get; set; }
        [Required]
        public required int SourceId { get; set; }
        [Required]
        public required DocumentType SourceType { get; set; }
        [Required]
        public required DocumentStatus Status { get; set; }
        [Required]
        public required string Wearhouse { get; set; }
        [Required]
        public required string Client { get; set; }
        public string? Description { get; set; }
        [Required]
        public required List<AddProductToDocumentDTO> Products { get; set; }
        public List<Attribute>? Attributes { get; set; }
    }
}
