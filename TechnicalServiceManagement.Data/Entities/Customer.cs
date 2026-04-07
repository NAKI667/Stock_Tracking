namespace TechnicalServiceManagement.Data.Entities;

public sealed class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<Device> Devices { get; set; } = [];
    public List<ServiceRequest> ServiceRequests { get; set; } = [];
}
