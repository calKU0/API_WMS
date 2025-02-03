using APIWMS.Data;
using APIWMS.Interfaces;
using APIWMS.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Numerics;

public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _context;
    private readonly IXlApiService _xlApiService;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(AppDbContext context, IXlApiService xlApiService, ILogger<DatabaseService> logger)
    {
        _context = context;
        _xlApiService = xlApiService;
        _logger = logger;
    }

    public async Task<List<string>> UpdateAttributes(int obiNumber, int obiType, int obiLp, List<APIWMS.Models.Attribute> attributes)
    {
        var failedUpdates = new List<string>();

        foreach (var attribute in attributes)
        {
            try
            {
                int rowsUpdated = 0;
                rowsUpdated = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC kkur.ZaktualizujAtrybut @ObjectId = {0}, @ObjectType = {1}, @ObjectLp = {2}, @Class = {3}, @Value = {4}",
                    obiNumber, obiType, obiLp, attribute.Name, attribute.Value);

                if (rowsUpdated == 0)
                {
                    int addResult = _xlApiService.AddAttribute(obiNumber, obiType, obiLp, attribute);
                    if (addResult != 0)
                        failedUpdates.Add($"{attribute.Name}. XLAPI error code: {addResult}");
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

    public async Task<int> UpdateProduct(Product product)
    {
        return await _context.Database.ExecuteSqlRawAsync(
        @"EXEC kkur.UpdateProduct @ProductId = {0}, @Ean = {1}, @Weight = {2}, @Volume = {3}, @VolumeUnit = {4}",
        product.ErpId, product.Ean, product.Weight, product.Volume, product.VolumeUnit);
    }

    public async Task<List<string>> UpdateProductUnits(int ProductId, List<ProductUnit> units)
    {
        var failedUpdates = new List<string>();

        foreach (var unit in units)
        {
            try
            {
                int jmLp = 0;
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC kkur.ZaktualizujJMTowaru @ProductId, @Jm, @Ean, @Weight, @Volume, @VolumeUnit, @Converter";
                    command.CommandType = CommandType.Text;

                    // Add parameters
                    command.Parameters.Add(new SqlParameter("@ProductId", ProductId));
                    command.Parameters.Add(new SqlParameter("@Jm", unit.Unit));
                    command.Parameters.Add(new SqlParameter("@Ean", unit.Ean ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Weight", unit.Weight));
                    command.Parameters.Add(new SqlParameter("@Volume", unit.Volume));
                    command.Parameters.Add(new SqlParameter("@VolumeUnit", unit.VolumeUnit));
                    command.Parameters.Add(new SqlParameter("@Converter", unit.Converter));

                    await _context.Database.OpenConnectionAsync();
                    jmLp = Convert.ToInt32(await command.ExecuteScalarAsync());
                    await _context.Database.CloseConnectionAsync();
                }

                if (jmLp > 0)
                {
                    var failedAttributes = await UpdateAttributes(ProductId, 16, jmLp, unit.Attributes);

                    if (failedAttributes.Count > 0)
                        failedUpdates.Add($"{unit.Unit}. Error updating attribute: {string.Join(", ", failedAttributes)}");
                }

                if (jmLp == 0)
                    failedUpdates.Add(unit.Unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unit: {UnitName}", unit.Unit);
                failedUpdates.Add(unit.Unit);
            }
        }

        return failedUpdates;
    }
}
