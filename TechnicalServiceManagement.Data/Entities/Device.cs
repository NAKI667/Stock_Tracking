namespace TechnicalServiceManagement.Data.Entities;

public sealed class Device
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public Customer? Customer { get; set; }
    public List<ServiceRequest> ServiceRequests { get; set; } = [];
}
