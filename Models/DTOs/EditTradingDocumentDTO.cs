using APIWMS.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace APIWMS.Models.DTOs
{
    public class EditTradingDocumentDTO
    {
        /// <summary>
        /// Document id in WMS system
        /// </summary>
        [Required]
        public required int WmsId { get; set; }
        /// <summary>
        /// Document type in WMS system
        /// </summary>
        [Required]
        public required int WmsType { get; set; }
        /// <summary>
        /// Document id in ERP system
        /// </summary>
        [Required]
        public required int ErpId { get; set; }
        /// <summary>
        /// Document type in ERP system
        /// </summary>
        [Required]
        public required TradingDocumentType ErpType { get; set; }
        /// <summary>
        /// Status of the ERP document
        /// </summary>
        public DocumentStatus? Status { get; set; }
        /// <summary>
        /// List of attributes of the document
        /// </summary>
        public List<Attribute>? Attributes { get; set; }
    }
}
