namespace TechnicalServiceManagement.Data.Entities;

public sealed class PartUsage
{
    public int Id { get; set; }
    public int ServiceRequestId { get; set; }
    public int SparePartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public ServiceRequest? ServiceRequest { get; set; }
    public SparePart? SparePart { get; set; }
}
