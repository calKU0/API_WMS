using APIWMS.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace APIWMS.Models.DTOs
{
    public class EditWarehouseDocumentDTO
    {
        /// <summary>
        /// Document id in ERP system
        /// </summary>
        [Required]
        public required int ErpId { get; set; }
        /// <summary>
        /// Document type in ERP system
        /// </summary>
        [Required]
        public required WarehouseDocumentType ErpType { get; set; }
        /// <summary>
        /// Status of the ERP document
        /// </summary>
        public DocumentStatus? Status { get; set; }
        /// <summary>
        /// List of products to add to document
        /// </summary>
        public List<DocumentProductDTO>? PositionsToAdd { get; set; }
        /// <summary>
        /// List of products to realize on document AWD or ZWM
        /// </summary>
        public List<PositionsToRealizeDTO>? PositionsToRealize { get; set; }
        /// <summary>
        /// List of attributes of the document
        /// </summary>
        public List<Attribute>? Attributes { get; set; }
    }
}
