using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models.DTOs
{
    public class DocumentProductDTO
    {
        /// <summary>
        /// Product code
        /// </summary>
        [Required]
        public required string Code { get; set; }
        /// <summary>
        /// Product quantity
        /// </summary>
        [Required]
        public required string Quantity { get; set; }
        /// <summary>
        /// Product unit
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// Batch id in ERP system
        /// </summary>
        public int BatchId { get; set; }
        /// <summary>
        /// Batch type in ERP system
        /// </summary>
        public int BatchType { get; set; }
    }
}
