using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models
{
    public class Product
    {
        [Required]
        public required int WmsId { get; set; }
        [Required]
        public required int ErpId { get; set; }
        public string? Ean { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
        public string? VolumeUnit { get; set; }
        public List<string>? Images { get; set; }
        public List<ProductUnit>? AdditionalUnits { get; set; }
        public List<Attribute>? Attributes { get; set; }
    }
}
