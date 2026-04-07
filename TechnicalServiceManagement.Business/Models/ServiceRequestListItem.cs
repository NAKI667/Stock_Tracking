namespace TechnicalServiceManagement.Business.Models;

public sealed class ServiceRequestListItem
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime IntakeDate { get; init; }
    public decimal TotalCost { get; init; }
}
