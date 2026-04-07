namespace TechnicalServiceManagement.Data.Entities;

public enum ServiceRequestStatus
{
    Received = 1,
    InProgress = 2,
    WaitingForPart = 3,
    Completed = 4,
    Delivered = 5
}
