using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models
{
    public class Attribute
    {
        /// <summary>
        /// Attribute class name
        /// </summary>
        [Required]
        public required string Name { get; set; }
        /// <summary>
        /// Attribute value
        /// </summary>
        [Required]
        public required string Value { get; set; }
    }
}
