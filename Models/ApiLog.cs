using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace APIWMS.Models
{
    public class ApiLog
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required int Id { get; set; }
        [Required]
        public required int EntityId { get; set; }
        [Required]
        public required int EntityType { get; set; }
        [Required]
        public required string Action { get; set; }
        [Required]
        public required bool Success { get; set; }
        public int? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
