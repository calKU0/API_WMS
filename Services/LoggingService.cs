﻿using APIWMS.Data;
using APIWMS.Data.Enums;
using APIWMS.Interfaces;
using APIWMS.Models;
using Serilog;

namespace APIWMS.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly AppDbContext _context;

        public LoggingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogErrorAsync(string action, bool success, string errorMessage = null, Dictionary<string, int> fields = null, Exception ex = null)
        {
            var log = new ApiLog
            {
                Action = action,
                Success = success,
                ErrorMessage = errorMessage,
                Flow = LogFlow.IN
            };

            if (fields != null)
            {
                foreach (var field in fields)
                {
                    switch (field.Key.ToLower())
                    {
                        case "entitywmsid":
                            log.EntityWmsId = Convert.ToInt32(field.Value);
                            break;
                        case "entitywmstype":
                            log.EntityWmsType = (EntityType)field.Value;
                            break;
                        case "entityerpid":
                            log.EntityErpId = Convert.ToInt32(field.Value);
                            break;
                        case "entityerptype":
                            log.EntityErpType = (EntityType)field.Value;
                            break;
                    }
                }
            }

            if (ex != null)
            {
                log.ErrorMessage += $" | Exception Details: {ex.Message}";
            }

            await _context.ApiLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
