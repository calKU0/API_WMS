using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models.DTOs
{
    public class AddProductToDocumentDTO
    {
        [Required]
        public required string Code { get; set; }
        [Required]
        public required string Quantity { get; set; }
    }
}
