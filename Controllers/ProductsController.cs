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
                await LogErrorAsync(actionName, "Nieprawidłowy stan modelu.", product.ErpId, productType, product.WmsId);
                return BadRequest(ModelState);
            }

            if (product.Attributes == null && product.Ean == null && product.Volume == null && product.WeightNetto == null && product.WeightBrutto == null)
            {
                const string errorMessage = "Brak danych do aktualizacji. Przekaż przynajmniej jedno pole do zaktualizowania.";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType, product.WmsId);
                return BadRequest(new { Message = errorMessage });
            }

            if (OnlyOneIsNull(product.WeightBrutto, product.WeightNetto))
            {
                const string errorMessage = "Podczas aktualizacji Wagi należy przekazać zarówno WageNetto, jak i WageBrutto.";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType, product.WmsId);
                return BadRequest(new { Message = errorMessage });
            }

            if (OnlyOneIsNull(product.Volume, product.VolumeUnit))
            {
                const string errorMessage = "Podczas aktualizacji objętości należy przekazać zarówno VolumeUnit, jak i Volume.";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType, product.WmsId);
                return BadRequest(new { Message = errorMessage });
            }

            try
            {
                var updateResults = new List<string>();

                int rowsAffected = _databaseService.UpdateProduct(product);
                updateResults.Add(rowsAffected > 0 ? "Dane towaru zaktualizowane pomyślnie." : "Nie udało się zaktualizować danych towaru.");

                if (product.Attributes != null)
                {
                    var failedAttributes = _databaseService.UpdateAttribute(product, product.Attributes);

                    if (failedAttributes.Any())
                        updateResults.AddRange(failedAttributes.Select(attr => $"Nie udało się zaktualizować atrybutu: {attr}."));
                    else
                        updateResults.Add("Atrybuty towaru zaktualizowane pomyślnie.");
                }

                var failedUpdates = updateResults.Where(result => result.Contains("Nie udało się")).ToList();

                if (failedUpdates.Any())
                {
                    foreach (var failure in failedUpdates)
                        await LogErrorAsync(actionName, $"Błąd przy próbie aktualizacji towaru: {failure}", product.ErpId, productType, product.WmsId);
                    return BadRequest(new { Message = "Wystąpił błąd podczas aktualizacji towaru.", Results = updateResults });
                }


                await LogSuccessAsync(actionName, product.ErpId, productType, product.WmsId);
                _logger.LogInformation("Zaktualizowano towar o ERPID {ProductId}: {Results}", product.ErpId, updateResults);
                return Ok(new { Message = "Towar zaktualizowany pomyślnie.", Results = updateResults });
            }
            catch (Exception ex)
            {
                const string errorMessage = "Wystąpił niezidentyfikowany błąd przy próbie aktualizacji danych towaru.";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType, product.WmsId, ex);
                return StatusCode(500, new { Message = "Wystąpił nieoczekiwany błąd." });
            }

        }

        private bool OnlyOneIsNull(object value1, object value2)
        {
            return (value1 == null && value2 != null) || (value1 != null && value2 == null);
        }

        private async Task LogErrorAsync(string actionName, string errorMessage, int erpId, int type, int wmsId, Exception ex = null)
        {
            await _loggingService.LogErrorAsync(actionName, false, errorMessage, new Dictionary<string, int> { { "EntityErpId", erpId }, { "EntityErpType", type }, { "EntityWmsId", wmsId }, { "EntityWmsType", type } }, ex);
            _logger.LogError(ex, errorMessage);
        }

        private async Task LogSuccessAsync(string actionName, int erpId, int type, int wmsId)
        {
            await _loggingService.LogErrorAsync(actionName, true, null, new Dictionary<string, int> { { "EntityErpId", erpId }, { "EntityErpType", type }, { "EntityWmsId", wmsId }, { "EntityWmsType", type } });
        }
    }
}
