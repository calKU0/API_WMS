using APIWMS.Data;
using APIWMS.Interfaces;
using APIWMS.Models.DTOs;
using APIWMS.Models;
using Microsoft.EntityFrameworkCore;

public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _context;

    public DatabaseService(AppDbContext context)
    {
        _context = context;
    }

    public int UpdateAttribute(Object obj, List<APIWMS.Models.Attribute> attributes)
    {
        int rowsUpdated = 0;

        foreach (var attribute in attributes)
        {
            if (obj is Product product)
            {
                rowsUpdated += _context.Database.ExecuteSqlRaw(
                "EXEC kkur.UpdateAttribute @ObjectId = {0}, @ObjectType = {1}, @Class = {2}, @Value = {3}",
                product.ErpId, 16, attribute.Name, attribute.Value);
            }
            if (obj is EditDocumentDTO document)
            {
                rowsUpdated += _context.Database.ExecuteSqlRaw(
                "EXEC kkur.UpdateAttribute @ObjectId = {0}, @ObjectType = {1}, @Class = {2}, @Value = {3}",
                document.ErpId, document.ErpType, attribute.Name, attribute.Value);
            }
        }

        return rowsUpdated;
    }
    public int UpdateProduct(Product product)
    {
        return _context.Database.ExecuteSqlRaw(
        "EXEC kkur.UpdateProduct @ProductId = {0}, @Ean = {1}, @WageNetto = {2}, @WageBrutto = {3}, @Volume = {4}, @VolumeUnit = {5}",
        product.ErpId, product.Ean, product.WageNetto, product.WageBrutto, product.Volume, product.VolumeUnit);
    }

}
