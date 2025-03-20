using APIWMS.Data;
using APIWMS.Data.Enums;
using APIWMS.Interfaces;
using APIWMS.Models;
using APIWMS.Models.DTOs;
using APIWMS.Models.ViewModels;
using APIWMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace APIWMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IXlApiService _xlApi;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IXlApiService xlApi, ILoggingService loggingService, ILogger<DocumentsController> logger)
        {
            _xlApi = xlApi;
            _loggingService = loggingService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new warehouse document.
        /// </summary>
        /// <returns>A document.</returns>
        /// <response code="200">A newly created document.</response>
        /// <response code="400">Document has invalid data.</response>
        /// <response code="500">Internal server error.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("CreateWarehouseDocument")]
        [HttpPost]
        public async Task<ActionResult<CreateWarehouseDocumentDTO>> CreateWearhouseDocument(CreateWarehouseDocumentDTO document)
        {
            const string action = "CreateWarehouseDocument";

            if (!ModelState.IsValid)
            {
                await LogErrorAsync(action, "Invalid model structure passed.", document);
                return BadRequest(ModelState);
            }

            if (!Enum.IsDefined(typeof(TradingDocumentType), document.ErpType))
            {
                await LogErrorAsync(action, $"Document type '{document.ErpType}' is not recognized.", document);
                return BadRequest(new { Message = $"Document type '{document.ErpType}' is not recognized." });
            }

            try
            {
                string result = _xlApi.CreateWarehouseDocument(document);
                if (!string.IsNullOrEmpty(result))
                {
                    await LogErrorAsync(action, result, document);
                    return BadRequest(new { Message = $"Error occured: {result}" });
                }

                await LogSuccessAsync(action, document);
                return Ok(document);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(action, "Unexpected error occured.", document, ex);
                return StatusCode(500, new { Message = "Unexpected error occured." });
            }
        }

        /// <summary>
        /// Modifies a warehouse document.
        /// </summary>
        /// <returns>A document.</returns>
        /// <response code="200">A modified document.</response>
        /// <response code="400">Document has invalid data.</response>
        /// <response code="500">Internal server error.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("EditWarehouseDocument")]
        [HttpPost]
        public async Task<ActionResult<EditWarehouseDocumentDTO>> EditWarehouseDocument(EditWarehouseDocumentDTO document)
        {
            const string action = "EditWarehouseDocument";

            if (!ModelState.IsValid)
            {
                await LogErrorAsync(action, "Invalid model structure passed.", document);
                return BadRequest(ModelState);
            }

            if (!Enum.IsDefined(typeof(TradingDocumentType), document.ErpType))
            {
                string errorMessage = $"Document type '{document.ErpType}' is not recognized.";
                await LogErrorAsync(action, errorMessage, document);
                return BadRequest(new { Message = errorMessage });
            }

            try
            {
                string result = await _xlApi.EditWarehouseDocument(document);
                if (!string.IsNullOrEmpty(result))
                {
                    await LogErrorAsync(action, result, document);
                    return BadRequest(new { Message = $"Error occured: {result}" });
                }

                await LogSuccessAsync(action, document);
                return Ok(new { document, Message = "Document modified" });
            }
            catch (Exception ex)
            {
                await LogErrorAsync(action, "Unexpected error occured.", document, ex);
                return StatusCode(500, new { Message = "Unexpected error occured." });
            }
        }

        /// <summary>
        /// Creates a new trading document.
        /// </summary>
        /// <returns>A document.</returns>
        /// <response code="200">A newly created document.</response>
        /// <response code="400">Document has invalid data.</response>
        /// <response code="500">Internal server error.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("CreateTradingDocument")]
        [HttpPost]
        public async Task<ActionResult<CreateTradingDocumentDTO>> CreateTradingDocument(CreateTradingDocumentDTO document)
        {
            const string action = "CreateTradingDocument";

            if (!ModelState.IsValid)
            {
                await LogErrorAsync(action, "Invalid model structure passed.", document);
                return BadRequest(ModelState);
            }

            if (!Enum.IsDefined(typeof(TradingDocumentType), document.ErpType))
            {
                await LogErrorAsync(action, $"Document type '{document.ErpType}' is not recognized.", document);
                return BadRequest(new { Message = $"Document type '{document.ErpType}' is not recognized." });
            }

            try
            {
                string result = _xlApi.CreateTradingDocument(document);
                if (!string.IsNullOrEmpty(result))
                {
                    await LogErrorAsync(action, result, document);
                    return BadRequest(new { Message = $"Error occured: {result}" });
                }

                await LogSuccessAsync(action, document);
                return Ok(document);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(action, "Unexpected error occured.", document, ex);
                return StatusCode(500, new { Message = "Unexpected error occured." });
            }
        }

        /// <summary>
        /// Modifies a trading document.
        /// </summary>
        /// <returns>A document.</returns>
        /// <response code="200">A modified document.</response>
        /// <response code="400">Document has invalid data.</response>
        /// <response code="500">Internal server error.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("EditTradingDocument")]
        [HttpPost]
        public async Task<ActionResult<EditTradingDocumentDTO>> EditTradingDocument(EditTradingDocumentDTO document)
        {
            const string action = "EditTradingDocument";

            if (!ModelState.IsValid)
            {
                await LogErrorAsync(action, "Invalid model structure passed.", document);
                return BadRequest(ModelState);
            }

            if (!Enum.IsDefined(typeof(TradingDocumentType), document.ErpType))
            {
                string errorMessage = $"Document type '{document.ErpType}' is not recognized.";
                await LogErrorAsync(action, errorMessage, document);
                return BadRequest(new { Message = errorMessage });
            }

            try
            {
                string result = await _xlApi.EditTradingDocument(document);
                if (!string.IsNullOrEmpty(result))
                {
                    await LogErrorAsync(action, result, document);
                    return BadRequest(new { Message = $"Error occured: {result}" });
                }

                await LogSuccessAsync(action, document);
                return Ok(new { document, Message = "Document modified" });
            }
            catch (Exception ex)
            {
                await LogErrorAsync(action, "Unexpected error occured.", document, ex);
                return StatusCode(500, new { Message = "Unexpected error occured." });
            }
        }

        private async Task LogErrorAsync(string action, string message, object document, Exception ex = null)
        {
            var logFields = new Dictionary<string, int>();

            if (document is CreateWarehouseDocumentDTO createDocument)
            {
                logFields.Add("entitywmsid", createDocument.WmsId);
                logFields.Add("entitywmsType", createDocument.WmsType);
            }
            else if (document is EditWarehouseDocumentDTO editDocument)
            {
                logFields.Add("entitywmsid", editDocument.WmsId);
                logFields.Add("entitywmsType", editDocument.WmsType);
                logFields.Add("entityerpid", editDocument.ErpId);
                logFields.Add("entityerptype", (int)editDocument.ErpType);
            }

            _logger.LogError(ex, message);
            await _loggingService.LogErrorAsync(action, false, message, logFields, ex);
        }

        private async Task LogSuccessAsync(string action, object document)
        {
            var logFields = new Dictionary<string, int>();

            if (document is CreateWarehouseDocumentDTO createDocument)
            {
                logFields.Add("entitywmsid", createDocument.WmsId);
                logFields.Add("entitywmsType", createDocument.WmsType);
            }
            else if (document is EditWarehouseDocumentDTO editDocument)
            {
                logFields.Add("entitywmsid", editDocument.WmsId);
                logFields.Add("entitywmsType", editDocument.WmsType);
                logFields.Add("entityerpid", editDocument.ErpId);
                logFields.Add("entityerptype", (int)editDocument.ErpType);
            }

            await _loggingService.LogErrorAsync(action, true, null, logFields);
        }

    }
}
