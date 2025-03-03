using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models
{
    public class Product
    {
        /// <summary>
        /// Product id in WMS system
        /// </summary>
        [Required]
        public required int WmsId { get; set; }
        /// <summary>
        /// Product id in ERP system
        /// </summary>
        [Required]
        public required int ErpId { get; set; }
        /// <summary>
        /// Product code in ERP system
        /// </summary>
        [Required]
        public required string Code { get; set; }
        /// <summary>
        /// Product EAN
        /// </summary>
        public string? Ean { get; set; }
        /// <summary>
        /// Product weight
        /// </summary>
        public decimal? Weight { get; set; }
        /// <summary>
        /// Product volume
        /// </summary>
        public decimal? Volume { get; set; }
        /// <summary>
        /// Volume unit
        /// </summary>
        public string? VolumeUnit { get; set; }
        /// <summary>
        /// List of product images
        /// </summary>
        public List<string>? Images { get; set; }
        /// <summary>
        /// List of product additional units
        /// </summary>
        public List<ProductUnit>? AdditionalUnits { get; set; }
        /// <summary>
        /// List of product attributes
        /// </summary>
        public List<Attribute>? Attributes { get; set; }
    }
}
