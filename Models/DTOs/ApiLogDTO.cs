using APIWMS.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace APIWMS.Models.DTOs
{
    public class ApiLogDTO
    {
        /// <summary>
        /// WMS Entity Id
        /// </summary>
        public int EntityWmsId { get; set; }
        /// <summary>
        /// WMS Entity Type
        /// </summary>
        public EntityType EntityWmsType { get; set; }
        /// <summary>
        /// ERP Entity Id
        /// </summary>
        public int EntityErpId { get; set; }
        /// <summary>
        /// ERP Entity Type
        /// </summary>
        public EntityType EntityErpType { get; set; }
        /// <summary>
        /// Entity synchro flow. IN - Into WMS; OUT - Into ERP
        /// </summary>
        public LogFlow Flow { get; set; }
        /// <summary>
        /// Action/Method
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// Does the synchro succeeded
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Error message when the synchro didn't succeed
        /// </summary>
        public string? ErrorMessage { get; set; }
        /// <summary>
        /// Log created date
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
