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

        /// <summary>
        /// Gets all logs from database.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///         GET /api/Logs/GetLogs
        ///         
        /// </remarks>
        /// <returns>A list of logs.</returns>
        /// <response code="200">A list of logs.</response>
        [Route("GetLogs")]
        [HttpGet]
        public ActionResult<ApiLog> GetLogs()
        {
            return Ok(_context.ApiLogs.ToList());
        }

        /// <summary>
        /// Gets a log with specific entityId.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///         GET /api/Logs/GetLog?entityId=1?entityType=1?Source=ERP
        ///         
        /// </remarks>
        /// <returns>A log.</returns>
        /// <response code="200">A log.</response>
        /// <response code="400">Passed invalid data to parameters.</response>
        /// <response code="404">Log with specified parameters not found.</response>
        [Route("GetLog")]
        [HttpGet]
        public ActionResult<ApiLog> GetLog(int entityId, int entityType, string Source)
        {
            if (entityId <= 0 || entityType <= 0)
                return BadRequest();
            if (Source != "WMS" && Source != "ERP")
                return BadRequest();

            List<ApiLog> log;
            if (Source == "ERP")
                log = _context.ApiLogs.Where(i => i.EntityErpId == entityId && i.EntityErpType == entityType).ToList();
            else
                log = _context.ApiLogs.Where(i => i.EntityWmsId == entityId && i.EntityWmsType == entityType).ToList();

            if (log == null)
                return NotFound();

            return Ok(log);
        }
    }
}
