namespace TechnicalServiceManagement.Business.Models;

public sealed class ServiceRequestDetailsViewModel
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
    public string ProblemDescription { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal TotalCost { get; init; }
    public IReadOnlyList<string> Operations { get; init; } = [];
    public IReadOnlyList<string> PartUsages { get; init; } = [];
}
