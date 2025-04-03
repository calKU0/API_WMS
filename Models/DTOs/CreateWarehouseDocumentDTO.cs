using APIWMS.Data.Enums;
using APIWMS.Models.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace APIWMS.Models.ViewModels
{
    public class CreateWarehouseDocumentDTO
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
        public required WMSDocumentType WmsType { get; set; }
        /// <summary>
        /// Document name in WMS system
        /// </summary>
        [Required]
        public required string WmsName { get; set; }
        /// <summary>
        /// ERP ID - This field is only for responses, not for incoming POST requests.
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public int ErpId { get; set; }
        /// <summary>
        /// Document type in ERP system
        /// </summary>
        [Required]
        public required WarehouseDocumentType ErpType { get; set; }
        /// <summary>
        /// Document id of the related document
        /// </summary>
        [Required]
        public required int SourceDocId { get; set; }
        /// <summary>
        /// Document type of the related document
        /// </summary>
        [Required]
        public required TradingDocumentType SourceDocType { get; set; }
        /// <summary>
        /// Status of the ERP document
        /// </summary>
        [Required]
        public required DocumentStatus Status { get; set; }
        /// <summary>
        /// Wearhouse
        /// </summary>
        [Required]
        public required Warehouse Warehouse { get; set; }
        /// <summary>
        /// Id of the ERP client
        /// </summary>
        [Required]
        public required int ClientId { get; set; }
        /// <summary>
        /// Acronym of the ERP client
        /// </summary>
        [Required]
        public required string ClientName { get; set; }
        /// <summary>
        /// Id of the target ERP client
        /// </summary>
        public int TargetClientId { get; set; }
        /// <summary>
        /// Description of the document
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// List of products on the document
        /// </summary>
        [Required]
        public required List<DocumentProductDTO> Products { get; set; }
        /// <summary>
        /// List of attributes of the document
        /// </summary>
        public List<Attribute>? Attributes { get; set; }
    }
}
