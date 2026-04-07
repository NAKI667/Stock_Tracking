namespace TechnicalServiceManagement.Data.Entities;

public sealed class ServiceRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int DeviceId { get; set; }
    public string ProblemDescription { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime IntakeDate { get; set; } = DateTime.Now;
    public DateTime? CompletedDate { get; set; }
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Received;
    public decimal LaborCost { get; set; }
    public bool IsPaid { get; set; }
    public Customer? Customer { get; set; }
    public Device? Device { get; set; }
    public List<ServiceOperation> ServiceOperations { get; set; } = [];
    public List<PartUsage> PartUsages { get; set; } = [];
}
