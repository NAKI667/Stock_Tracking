namespace TechnicalServiceManagement.Data.Entities;

public sealed class ServiceOperation
{
    public int Id { get; set; }
    public int ServiceRequestId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime OperationDate { get; set; } = DateTime.Now;
    public decimal Cost { get; set; }
    public ServiceRequest? ServiceRequest { get; set; }
}
