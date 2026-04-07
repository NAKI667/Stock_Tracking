namespace TechnicalServiceManagement.Data.Entities;

public sealed class SparePart
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public List<PartUsage> PartUsages { get; set; } = [];
}
