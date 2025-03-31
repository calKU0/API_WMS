using APIWMS.Data.Enums;
using APIWMS.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace APIWMS.Models.ViewModels
{
    public class CreateTradingDocumentDTO
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
        /// Document name in WMS system
        /// </summary>
        [Required]
        public required string WmsName { get; set; }
        /// <summary>
        /// ERP ID - This field is only for responses, not for incoming POST requests.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int ErpId { get; set; }
        /// <summary>
        /// Document type in ERP system
        /// </summary>
        [Required]
        public required TradingDocumentType ErpType { get; set; }
        /// <summary>
        /// Document id of the related document
        /// </summary>
        public int SourceDocId { get; set; }
        /// <summary>
        /// Document type of the related document
        /// </summary>
        public TradingDocumentType SourceDocType { get; set; }
        /// <summary>
        /// Source warehouse
        /// </summary>
        [Required]
        public required Warehouse SourceWarehouse { get; set; }
        /// <summary>
        /// Destination warehouse
        /// </summary>
        /// 
        [Required]
        public required Warehouse DestinationWarehouse { get; set; }
        /// <summary>
        /// Status of the ERP document
        /// </summary>
        /// 
        [Required]
        public required DocumentStatus Status { get; set; }
        /// <summary>
        /// Id of the ERP client
        /// </summary>
        [Required]
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
