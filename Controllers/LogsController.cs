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
        public async Task<ActionResult<List<ApiLogDTO>>> GetLogs()
        {
            try
            {
                var logs = await _context.ApiLogs
                    .Select(log => new ApiLogDTO
                    {
                        EntityErpId = log.EntityErpId,
                        EntityErpType = log.EntityErpType,
                        EntityWmsId = log.EntityWmsId,
                        EntityWmsType = log.EntityWmsType,
                        Action = log.Action,
                        Success = log.Success,
                        ErrorMessage = log.ErrorMessage,
                        CreatedDate = log.CreatedDate
                    })
                    .ToListAsync();

                if (!logs.Any())
                {
                    return NotFound("No logs found.");
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error fetching logs: {ex}");
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
        public async Task<ActionResult<ApiLogDTO>> GetLog(int entityId, DocumentType? entityType, string Source)
        {
            try
            {
                if (Source != "WMS" && Source != "ERP")
                    return BadRequest("Source can be only in list of [ERP, WMS]");

                List<ApiLogDTO> logs;
                if (Source == "ERP")
                {
                    logs = await _context.ApiLogs
                        .Where(i => i.EntityErpId == entityId && i.EntityErpType == entityType)
                        .Select(log => new ApiLogDTO
                        {
                            EntityErpId = log.EntityErpId,
                            EntityErpType = log.EntityErpType,
                            EntityWmsId = log.EntityWmsId,
                            EntityWmsType = log.EntityWmsType,
                            Action = log.Action,
                            Success = log.Success,
                            ErrorMessage = log.ErrorMessage,
                            CreatedDate = log.CreatedDate
                        })
                        .ToListAsync();
                }
                else
                {
                    logs = await _context.ApiLogs
                        .Where(i => i.EntityWmsId == entityId && i.EntityWmsType == entityType)
                         .Select(log => new ApiLogDTO
                         {
                             EntityErpId = log.EntityErpId,
                             EntityErpType = log.EntityErpType,
                             EntityWmsId = log.EntityWmsId,
                             EntityWmsType = log.EntityWmsType,
                             Action = log.Action,
                             Success = log.Success,
                             ErrorMessage = log.ErrorMessage,
                             CreatedDate = log.CreatedDate
                         })
                        .ToListAsync();
                }

                if (!logs.Any())
                    return NotFound();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error fetching logs: {ex}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
