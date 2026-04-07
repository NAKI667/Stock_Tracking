using TechnicalServiceManagement.Data;
using TechnicalServiceManagement.Data.Entities;

namespace TechnicalServiceManagement.Business.Managers;

public sealed class SparePartManager
{
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
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Part name is required.");
        }

        if (string.IsNullOrWhiteSpace(stockCode))
        {
            throw new InvalidOperationException("Stock code is required.");
        }

        if (unitPrice < 0)
        {
            throw new InvalidOperationException("Unit price cannot be negative.");
        }

        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        var normalizedName = name.Trim();
        var normalizedCode = stockCode.Trim().ToUpperInvariant();

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
        return existingPart;
    }
}
