using APIWMS.Data;
using APIWMS.Data.Enums;
using APIWMS.Interfaces;
using APIWMS.Models;
using APIWMS.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIWMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ILogger<LogsController> _logger;
        private readonly AppDbContext _context;
        public LogsController(AppDbContext context, ILogger<LogsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Route("GetLogs")]
        [HttpGet]
        public ActionResult<ApiLog> GetLogs()
        {
            return Ok(_context.ApiLogs.ToList());
        }

        [Route("GetLog/{entityId}/{entityType}")]
        [HttpGet]
        public ActionResult<ApiLog> GetLog(int entityId, int entityType, string Source)
        {
            if (entityId <= 0 || entityType <= 0)
                return BadRequest();
            if (Source != "WMS" && Source != "ERP")
                return BadRequest();

            ApiLog log;
            if (Source == "ERP")
                log = _context.ApiLogs.FirstOrDefault(i => i.EntityErpId == entityId && i.EntityErpType == entityType);
            else
                log = _context.ApiLogs.FirstOrDefault(i => i.EntityWmsId == entityId && i.EntityWmsType == entityType);

            if (log == null)
                return NotFound();

            return Ok(log);
        }
    }
}
