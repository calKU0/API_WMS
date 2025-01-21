using APIWMS.Data;
using APIWMS.Interfaces;
using APIWMS.Models.DTOs;
using APIWMS.Models;
using Microsoft.EntityFrameworkCore;

public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(AppDbContext context, ILogger<DatabaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<string> UpdateAttribute(Object obj, List<APIWMS.Models.Attribute> attributes)
    {
        var failedUpdates = new List<string>();

        foreach (var attribute in attributes)
        {
            try
            {
                int rowsUpdated = 0;

                if (obj is Product product)
                {
                    rowsUpdated += _context.Database.ExecuteSqlRaw(
                        "EXEC kkur.UpdateAttribute @ObjectId = {0}, @ObjectType = {1}, @Class = {2}, @Value = {3}",
                        product.ErpId, 16, attribute.Name, attribute.Value);
                }
                else if (obj is EditDocumentDTO document)
                {
                    rowsUpdated += _context.Database.ExecuteSqlRaw(
                        "EXEC kkur.UpdateAttribute @ObjectId = {0}, @ObjectType = {1}, @Class = {2}, @Value = {3}",
                        document.ErpId, document.ErpType, attribute.Name, attribute.Value);
                }

                if (rowsUpdated == 0)
                {
                    failedUpdates.Add(attribute.Name); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attribute: {AttributeName}", attribute.Name);
                failedUpdates.Add(attribute.Name); 
            }
        }

        return failedUpdates;
    }

    public int UpdateProduct(Product product)
    {
        return _context.Database.ExecuteSqlRaw(
        "EXEC kkur.UpdateProduct @ProductId = {0}, @Ean = {1}, @WageNetto = {2}, @WageBrutto = {3}, @Volume = {4}, @VolumeUnit = {5}",
        product.ErpId, product.Ean, product.WeightNetto, product.WeightBrutto, product.Volume, product.VolumeUnit);
    }

}
