namespace TechnicalServiceManagement.Business.Models;

public sealed class DashboardSummary
{
    public int TotalRequests { get; init; }
    public int ActiveRequests { get; init; }
    public int FinishedRequests { get; init; }
    public int LowStockParts { get; init; }
}
