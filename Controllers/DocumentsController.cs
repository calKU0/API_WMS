using APIWMS.Data.Enums;
using APIWMS.Models.DTOs;
using APIWMS.Models.ViewModels;
using APIWMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIWMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IXlApiService _xlApi;
        private readonly ILogger<DocumentsController> _logger;
        public DocumentsController(IXlApiService xlApi, ILogger<DocumentsController> logger)
        {
            _xlApi = xlApi;
            _logger = logger;
        }

        [Route("CreateDocument")]
        [HttpPost]
        public ActionResult<CreateDocumentDTO> CreateDocument(CreateDocumentDTO document)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!Enum.IsDefined(typeof(DocumentType), document.Type))
                return NotFound(new { Message = $"Document type '{document.Type}' is not recognized." });
            if (document == null)
                return BadRequest(document);

            string result = _xlApi.CreateDocument(document);

            if (!String.IsNullOrEmpty(result))
                return BadRequest(new { Message = $"Error occured: {result}" });

            return Ok(document);
        }
        [Route("EditDocument")]
        [HttpPost]
        public ActionResult<EditDocumentDTO> EditDocument(EditDocumentDTO document)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!Enum.IsDefined(typeof(DocumentType), document.Type))
                return NotFound(new { Message = $"Document type '{document.Type}' is not recognized." });
            if (document == null)
                return BadRequest(document);

            string result = _xlApi.ModifyDocument(document);

            if (!String.IsNullOrEmpty(result))
                return BadRequest(new { Message = $"Error occured: {result}" });

            return Ok(document);
        }
    }
}
