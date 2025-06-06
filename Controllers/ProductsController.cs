﻿using APIWMS.Data.Enums;
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

        /// <summary>
        /// Modifies a product.
        /// </summary>
        /// <remarks>
        /// Updates the details and attributes of a product in the system.  
        /// The modification may include changes to:  
        ///  
        /// - Product EAN, Weight, Volume, Images, Units
        /// - Custom attributes specific to the product
        ///  
        /// Available attributes:  
        /// - **Wysokość magazynowa (cm)**: Height of the product’s base unit. 
        /// - **Długość magazynowa (cm)**: Length of the product’s base unit.  
        /// - **Szerokość magazynowa (cm)**: Width of the product’s base unit. 
        /// - **Grupa WMS**: Specifies the WMS group.
        /// - **Przenośnik taśmowy** (`TAK`, `NIE`): Specifies whether the product should be packed on a pallet (`NO`) or in packages (`YES`).
        /// - **Typ towaru pack** (`Standardowy`, `Niestandardowy`, `Długi do 2mb`, `Długi do 3mb`, `Gabarytowy`, `Delikatny`, `Paletowy`, `Pół-paletowy`, `Standardowy/Nieokreslony`)
        /// 
        ///  Available attributes for product unit:  
        /// - **Wysokość magazynowa (cm)**: Height of the product’s unit. 
        /// - **Długość magazynowa (cm)**: Length of the product’s unit.  
        /// - **Szerokość magazynowa (cm)**: Width of the product’s unit. 
        /// 
        /// The response returns a list of update results indicating the success or failure of each modification.  
        /// If invalid data is provided, a `400 Bad Request` response is returned.  
        /// </remarks>
        /// <returns>A list of update results.</returns>
        /// <response code="200">A list of update results.</response>
        /// <response code="400">Invalid data provided.</response>
        /// <response code="500">Internal server error.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("UpdateProduct")]
        [HttpPost]
        public async Task<ActionResult> UpdateProduct(Product product)
        {
            const string actionName = "UpdateProduct";
            const int productType = 16;

            if (!ModelState.IsValid)
            {
                await LogErrorAsync(actionName, "Invalid model struvture passed.", product.ErpId, productType, product.WmsId);
                return BadRequest(ModelState);
            }

            if (product.Attributes == null && product.Ean == null && product.Volume == null && product.Weight == null)
            {
                const string errorMessage = "No data to update. Pass at least one value.";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType, product.WmsId);
                return BadRequest(new { Message = errorMessage });
            }

            if (OnlyOneIsNull(product.Volume, product.VolumeUnit))
            {
                const string errorMessage = "When updating volume, Volume Unit is required";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType, product.WmsId);
                return BadRequest(new { Message = errorMessage });
            }

            try
            {
                var updateResults = new List<string>();

                int rowsAffected = await _databaseService.UpdateProduct(product);

                if (product.Attributes != null)
                {
                    var failedAttributes = await _databaseService.UpdateAttributes(product.ErpId, 16, 0, product.Attributes);

                    if (failedAttributes.Count > 1)
                        updateResults.AddRange(failedAttributes.Select(attr => $"Error when updating attribute: '{attr}'."));
                }

                if (product.AdditionalUnits != null)
                {
                    var failedUnits = await _databaseService.UpdateProductUnits(product.ErpId, product.AdditionalUnits);

                    if (failedUnits.Count > 0)
                        updateResults.AddRange(failedUnits.Select(jm => $"Error when updating unit: '{jm}'."));
                }

                var failedUpdates = updateResults.Where(result => result.Contains("Error")).ToList();

                if (failedUpdates.Count > 0)
                {
                    foreach (var failure in failedUpdates)
                        await LogErrorAsync(actionName, $"Error when updating product: {failure}", product.ErpId, productType, product.WmsId);
                    return BadRequest(new { Message = "Error when updating product.", Results = updateResults });
                }

                await LogSuccessAsync(actionName, product.ErpId, productType, product.WmsId);
                _logger.LogInformation("Modified product with ERPID {ProductId}: {Results}", product.ErpId, updateResults);
                return Ok(new { product, Message = "Product modified." });
            }
            catch (Exception ex)
            {
                const string errorMessage = "Unexpected error occured when trying to modify product.";
                await LogErrorAsync(actionName, errorMessage, product.ErpId, productType, product.WmsId, ex);
                return StatusCode(500, new { Message = "Enexpected error occured" });
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
