using TechnicalServiceManagement.Data;
using TechnicalServiceManagement.Data.Entities;

namespace TechnicalServiceManagement.Business.Managers;

public sealed class SparePartManager
{
    private readonly AuditService _auditService = new();

    public IReadOnlyList<SparePart> GetSpareParts()
    {
        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();
        return dbContext.SpareParts
            .OrderBy(part => part.Name)
            .ToList();
    }

    public IReadOnlyList<SparePart> GetAvailableParts()
    {
        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();
        return dbContext.SpareParts
            .Where(part => part.StockQuantity > 0)
            .OrderBy(part => part.Name)
            .ToList();
    }

    public SparePart AddOrUpdateStock(string name, string stockCode, decimal unitPrice, int quantity)
    {
        var normalizedName = InputSanitizer.ValidateRequired(name, "Part name");
        var normalizedCode = InputSanitizer.ValidateRequired(stockCode, "Stock code")
            .ToUpperInvariant();

        InputSanitizer.ValidatePositiveAmount(unitPrice, "Unit price");
        InputSanitizer.ValidatePositiveInteger(quantity, "Quantity");

        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();
        var existingPart = dbContext.SpareParts
            .SingleOrDefault(part => part.StockCode == normalizedCode);

        if (existingPart is null)
        {
            existingPart = new SparePart
            {
                Name = normalizedName,
                StockCode = normalizedCode,
                UnitPrice = unitPrice,
                StockQuantity = quantity
            };

            dbContext.SpareParts.Add(existingPart);
        }
        else
        {
            existingPart.Name = normalizedName;
            existingPart.UnitPrice = unitPrice;
            existingPart.StockQuantity += quantity;
        }

        dbContext.SaveChanges();

        _auditService.LogAction("Stock", "SparePart", existingPart.Id,
            $"Stock updated: {normalizedName} ({normalizedCode}) qty +{quantity}, price {unitPrice:C}");

        return existingPart;
    }
}
