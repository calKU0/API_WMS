using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models
{
    public class ProductUnit
    {
        [Required]
        public required string Unit { get; set; }
        [Required]
        public required string Ean { get; set; }
        [Required]
        public decimal Weight { get; set; }
        [Required]
        public decimal Volume { get; set; }
        [Required]
        public required string VolumeUnit { get; set; }
        [Required]
        public required int Converter { get; set; }
        [Required]
        public required List<Attribute> Attributes { get; set; }
    }
}
