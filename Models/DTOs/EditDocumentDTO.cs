using APIWMS.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace APIWMS.Models.DTOs
{
    public class EditDocumentDTO
    {
        [Required]
        public required int Id { get; set; }
        [Required]
        public required DocumentType Type { get; set; }
        public string? Status { get; set; }
        public List<Attribute>? Attributes { get; set; }
    }
}
