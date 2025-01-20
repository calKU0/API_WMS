using APIWMS.Data.Enums;
using APIWMS.Interfaces;
using APIWMS.Models;
using APIWMS.Models.ViewModels;
using APIWMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIWMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IDatabaseService databaseService, ILoggingService loggingService, ILogger<ProductsController> logger)
        {
            _databaseService = databaseService;
            _loggingService = loggingService;
            _logger = logger;
        }

        [Route("UpdateProduct")]
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(Product product)
        {
            const string actionName = "UpdateProduct";
            const int productType = 16;

            if (!ModelState.IsValid)
            {
                await LogErrorAsync(actionName, "Nieprawidłowy stan modelu.", product.ErpId, productType);
                return BadRequest(ModelState);
            }

            if (product.Attributes == null && product.Ean == null && product.Volume == null && product.WageNetto == null && product.WageBrutto == null)
            {
                const string errorMessage = "Brak danych do aktualizacji. Przekaż przynajmniej jedno pole do zaktualizowania.";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType);
                return BadRequest(new { Message = errorMessage });
            }

            if (OnlyOneIsNull(product.WageBrutto, product.WageNetto))
            {
                const string errorMessage = "Podczas aktualizacji Wagi należy przekazać zarówno WageNetto, jak i WageBrutto.";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType);
                return BadRequest(new { Message = errorMessage });
            }

            if (OnlyOneIsNull(product.Volume, product.VolumeUnit))
            {
                const string errorMessage = "Podczas aktualizacji objętości należy przekazać zarówno VolumeUnit, jak i Volume.";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType);
                return BadRequest(new { Message = errorMessage });
            }

            try
            {
                var updateResults = new List<string>();

                int rowsAffected = _databaseService.UpdateProduct(product);
                updateResults.Add(rowsAffected > 0 ? "Towar zaktualizowany pomyślnie." : "Nie udało się zaktualizować towaru.");

                if (product.Attributes != null)
                {
                    int attributesRowsAffected = _databaseService.UpdateAttribute(product, product.Attributes);
                    updateResults.Add(attributesRowsAffected > 0 ? "Atrybuty towaru zaktualizowane pomyślnie." : "Nie udało się zaktualizować atrybutów towaru.");
                }

                if (updateResults.Any(result => result.Contains("Nie udało się")))
                {
                    foreach (var row in updateResults.Where(result => result.Contains("Nie udało się")))
                    {
                        await LogErrorAsync(actionName, $"Błąd przy próbie aktualizacji towaru: {row}", product.ErpId, productType);
                    }
                    return BadRequest(new { Message = "Wystąpił błąd podczas aktualizacji towaru.", Results = updateResults });
                }

                await LogSuccessAsync(actionName, product.ErpId, productType);
                _logger.LogInformation("Zaktualizowano towar o ERPID {ProductId}: {Results}", product.ErpId, updateResults);
                return Ok(new { Message = "Towar zaktualizowany pomyślnie.", Results = updateResults });
            }
            catch (Exception ex)
            {
                const string errorMessage = "Wystąpił niezidentyfikowany błąd przy próbie aktualizacji danych towaru.";
                await _loggingService.LogErrorAsync(action: actionName, success: false, errorMessage: errorMessage,
                    fields: new Dictionary<string, int> { { "EntityErpId", product.ErpId }, { "EntityErpType", productType } },
                    ex: ex
                );
                return StatusCode(500, new { Message = "Wystąpił nieoczekiwany błąd." });
            }
        }

        private bool OnlyOneIsNull(object value1, object value2)
        {
            return (value1 == null && value2 != null) || (value1 != null && value2 == null);
        }

        private async Task LogErrorAsync(string actionName, string errorMessage, int erpId, int erpType)
        {
            await _loggingService.LogErrorAsync(action: actionName, success: false, errorMessage: errorMessage,
                fields: new Dictionary<string, int> { { "EntityErpId", erpId }, { "EntityErpType", erpType } }
            );
        }

        private async Task LogSuccessAsync(string actionName, int erpId, int erpType)
        {
            await _loggingService.LogErrorAsync(actionName, true, null,
                fields: new Dictionary<string, int> { { "EntityErpId", erpId }, { "EntityErpType", erpType } }
            );
        }
    }
}
