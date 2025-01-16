using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace APIWMS.Models.DTOs
{
    public class EditProductDTO
    {
        [Required]
        public required int ErpId { get; set; }
        [Required]
        public required string Code { get; set; }
        public string? Ean { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
        public List<Attribute>? Attributes { get; set; }
    }
}
