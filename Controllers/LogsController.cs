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
        /// <param name="flow">Entity synchro flow. IN - Into ERP; OUT - Into WMS (optional).</param>
        /// <param name="success">Filters logs by success status (optional).</param>
        /// <param name="dateFrom">Filters logs created on or after this date. Date must be passed in YYYY-MM-DDTHH:mm:ss format (optional).</param>
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
        public async Task<ActionResult<List<ApiLogDTO>>> GetLogs(LogFlow? flow, bool? success, DateTime? dateFrom)
        {
            try
            {
                var query = _context.ApiLogs.AsQueryable();

                if (success.HasValue)
                {
                    query = query.Where(i => i.Success == success.Value);
                }

                if (dateFrom.HasValue)
                {
                    query = query.Where(i => i.CreatedDate >= dateFrom.Value);
                }

                if (flow.HasValue)
                {
                    query = query.Where(i => i.Flow == flow);
                }

                var logs = await query.Select(log => new ApiLogDTO
                {
                    EntityErpId = log.EntityErpId,
                    EntityErpType = log.EntityErpType,
                    EntityWmsId = log.EntityWmsId,
                    EntityWmsType = log.EntityWmsType,
                    Flow = log.Flow,
                    Action = log.Action,
                    Success = log.Success,
                    ErrorMessage = log.ErrorMessage,
                    CreatedDate = log.CreatedDate
                }).ToListAsync();

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
        /// <param name="entityId">ID of the Entity.</param>
        /// <param name="entityType">Type of the Entity. (optional).</param>
        /// <param name="source">Source of the entity [ERP, WMS].</param>
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
        public async Task<ActionResult<List<ApiLogDTO>>> GetLog(int entityId, DocumentType? entityType, string source)
        {
            try
            {
                if (source != "WMS" && source != "ERP")
                    return BadRequest("Source can be only in list of [ERP, WMS]");

                var query = _context.ApiLogs.AsQueryable();

                if (source == "ERP")
                {
                    query = query.Where(i => i.EntityErpId == entityId);

                    if (entityType.HasValue)
                    {
                        query = query.Where(i => i.EntityErpType == entityType.Value);
                    }
                }
                else
                {
                    query = query.Where(i => i.EntityWmsId == entityId);

                    if (entityType.HasValue)
                    {
                        query = query.Where(i => i.EntityWmsType == entityType.Value);
                    }
                }

                var logs = await query.Select(log => new ApiLogDTO
                {
                    EntityErpId = log.EntityErpId,
                    EntityErpType = log.EntityErpType,
                    EntityWmsId = log.EntityWmsId,
                    EntityWmsType = log.EntityWmsType,
                    Flow = log.Flow,
                    Action = log.Action,
                    Success = log.Success,
                    ErrorMessage = log.ErrorMessage,
                    CreatedDate = log.CreatedDate
                }).ToListAsync();

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
