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

        [Route("CreateDocument")]
        [HttpPost]
        public async Task<ActionResult<CreateDocumentDTO>> CreateDocument(CreateDocumentDTO document)
        {
            const string action = "CreateDocument";

            if (!ModelState.IsValid)
            {
                await LogErrorAsync(action, "Invalid model structure passed.", document);
                return BadRequest(ModelState);
            }

            if (!Enum.IsDefined(typeof(DocumentType), document.ErpType))
            {
                await LogErrorAsync(action, $"Document type '{document.ErpType}' is not recognized.", document);
                return NotFound(new { Message = $"Document type '{document.ErpType}' is not recognized." });
            }

            try
            {
                string result = _xlApi.CreateDocument(document);
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

        [Route("EditDocument")]
        [HttpPost]
        public async Task<ActionResult<EditDocumentDTO>> EditDocument(EditDocumentDTO document)
        {
            const string action = "EditDocument";

            if (!ModelState.IsValid)
            {
                await LogErrorAsync(action, "Invalid model structure passed.", document);
                return BadRequest(ModelState);
            }

            if (!Enum.IsDefined(typeof(DocumentType), document.ErpType))
            {
                string errorMessage = $"Document type '{document.ErpType}' is not recognized.";
                await LogErrorAsync(action, errorMessage, document);
                return NotFound(new { Message = errorMessage });
            }

            try
            {
                string result = await _xlApi.ModifyDocument(document);
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

            if (document is CreateDocumentDTO createDocument)
            {
                logFields.Add("entitywmsid", createDocument.WmsId);
                logFields.Add("entitywmsType", createDocument.WmsType);
            }
            else if (document is EditDocumentDTO editDocument)
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

            if (document is CreateDocumentDTO createDocument)
            {
                logFields.Add("entitywmsid", createDocument.WmsId);
                logFields.Add("entitywmsType", createDocument.WmsType);
            }
            else if (document is EditDocumentDTO editDocument)
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
