using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models
{
    public class Attribute
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Value { get; set; }
    }
}
