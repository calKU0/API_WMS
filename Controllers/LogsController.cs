using APIWMS.Data;
using APIWMS.Data.Enums;
using APIWMS.Interfaces;
using APIWMS.Models;
using APIWMS.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        /// <returns>A list of logs.</returns>
        /// <response code="200">A list of logs.</response>
        /// <response code="404">No logs found.</response>
        /// <response code="500">Internal Server Error</response>
        [Route("GetLogs")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ApiLog>>> GetLogs()
        {
            try
            {
                var logs = await _context.ApiLogs.ToListAsync();

                if (!logs.Any())
                {
                    return NotFound("No logs found.");
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error fetching logs: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }


        /// <summary>
        /// Gets a log with specific entityId.
        /// </summary>
        /// <returns>A log.</returns>
        /// <response code="200">A log.</response>
        /// <response code="400">Passed invalid data to parameters.</response>
        /// <response code="404">Log with specified parameters not found.</response>
        /// <response code="500">Internal Server Error</response>
        [Route("GetLog")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiLog>> GetLog(int entityId, int entityType, string Source)
        {
            try
            {
                if (entityId <= 0 || entityType <= 0)
                    return BadRequest();
                if (Source != "WMS" && Source != "ERP")
                    return BadRequest();

                List<ApiLog> log;
                if (Source == "ERP")
                    log = await _context.ApiLogs.Where(i => i.EntityErpId == entityId && i.EntityErpType == entityType).ToListAsync();
                else
                    log = await _context.ApiLogs.Where(i => i.EntityWmsId == entityId && i.EntityWmsType == entityType).ToListAsync();

                if (log == null)
                    return NotFound();

                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error fetching logs: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
