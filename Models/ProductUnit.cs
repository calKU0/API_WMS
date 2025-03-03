using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models
{
    public class ProductUnit
    {
        /// <summary>
        /// Unit name
        /// </summary>
        [Required]
        public required string Unit { get; set; }
        /// <summary>
        /// Unit EAN
        /// </summary>
        [Required]
        public required string Ean { get; set; }
        /// <summary>
        /// Unit weight
        /// </summary>
        [Required]
        public decimal Weight { get; set; }
        /// <summary>
        /// Unit volume
        /// </summary>
        [Required]
        public decimal Volume { get; set; }
        /// <summary>
        /// Volume unit
        /// </summary>
        [Required]
        public required string VolumeUnit { get; set; }
        /// <summary>
        /// Converter to main unit
        /// </summary>
        [Required]
        public required int Converter { get; set; }
        /// <summary>
        /// List of unit attributes
        /// </summary>
        [Required]
        public required List<Attribute> Attributes { get; set; }
    }
}
