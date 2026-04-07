using Microsoft.EntityFrameworkCore;
using TechnicalServiceManagement.Business.Models;
using TechnicalServiceManagement.Data;
using TechnicalServiceManagement.Data.Entities;

namespace TechnicalServiceManagement.Business.Managers;

public sealed class ServiceRequestManager
{
    public IReadOnlyList<ServiceRequestListItem> GetServiceRequests()
    {
        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();

        var requests = dbContext.ServiceRequests
            .AsNoTracking()
            .Include(request => request.Customer)
            .Include(request => request.Device)
            .Include(request => request.ServiceOperations)
            .Include(request => request.PartUsages)
            .OrderByDescending(request => request.IntakeDate)
            .ToList();

        return requests
            .Select(request => new ServiceRequestListItem
            {
                Id = request.Id,
                CustomerName = request.Customer?.FullName ?? "Unknown",
                DeviceName = $"{request.Device?.Brand} {request.Device?.Model}".Trim(),
                Status = request.Status.ToString(),
                IntakeDate = request.IntakeDate,
                TotalCost = CalculateTotalCost(request)
            })
            .ToList();
    }

    public DashboardSummary GetDashboardSummary()
    {
        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();

        var requests = dbContext.ServiceRequests
            .AsNoTracking()
            .ToList();

        return new DashboardSummary
        {
            TotalRequests = requests.Count,
            ActiveRequests = requests.Count(request =>
                request.Status is ServiceRequestStatus.Received
                or ServiceRequestStatus.InProgress
                or ServiceRequestStatus.WaitingForPart),
            FinishedRequests = requests.Count(request =>
                request.Status is ServiceRequestStatus.Completed
                or ServiceRequestStatus.Delivered),
            LowStockParts = dbContext.SpareParts.Count(part => part.StockQuantity <= 3)
        };
    }

    public IReadOnlyList<string> GetAvailableStatuses()
    {
        return Enum.GetNames<ServiceRequestStatus>();
    }

    public ServiceRequest CreateServiceRequest(
        int customerId,
        string brand,
        string model,
        string serialNumber,
        string problemDescription)
    {
        if (customerId <= 0)
        {
            throw new InvalidOperationException("A customer must be selected.");
        }

        if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(model))
        {
            throw new InvalidOperationException("Device brand and model are required.");
        }

        if (string.IsNullOrWhiteSpace(problemDescription))
        {
            throw new InvalidOperationException("Problem description is required.");
        }

        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            throw new InvalidOperationException("Serial number is required.");
        }

        var normalizedBrand = brand.Trim();
        var normalizedModel = model.Trim();
        var normalizedSerialNumber = serialNumber.Trim().ToUpperInvariant();

        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();

        var customer = dbContext.Customers.Find(customerId)
            ?? throw new InvalidOperationException("Selected customer could not be found.");

        var existingDevice = dbContext.Devices.FirstOrDefault(device =>
            device.SerialNumber.ToUpper() == normalizedSerialNumber);

        if (existingDevice is not null && existingDevice.CustomerId != customerId)
        {
            throw new InvalidOperationException("This serial number is already registered for another customer.");
        }

        var device = existingDevice ?? new Device
        {
            CustomerId = customerId,
            Brand = normalizedBrand,
            Model = normalizedModel,
            SerialNumber = normalizedSerialNumber
        };

        if (existingDevice is null)
        {
            dbContext.Devices.Add(device);
        }

        var serviceRequest = new ServiceRequest
        {
            CustomerId = customerId,
            Device = device,
            ProblemDescription = problemDescription.Trim(),
            IntakeDate = DateTime.Now,
            Status = ServiceRequestStatus.Received,
            Notes = $"Accepted by reception for {customer.FullName}.",
            LaborCost = 0m,
            IsPaid = false
        };

        dbContext.ServiceRequests.Add(serviceRequest);
        dbContext.SaveChanges();

        return serviceRequest;
    }

    public void UpdateStatus(int requestId, string statusText)
    {
        if (!Enum.TryParse<ServiceRequestStatus>(statusText, out var status))
        {
            throw new InvalidOperationException("Selected status is invalid.");
        }

        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();
        var request = dbContext.ServiceRequests.Find(requestId)
            ?? throw new InvalidOperationException("Service request not found.");

        request.Status = status;

        if (status is ServiceRequestStatus.Completed or ServiceRequestStatus.Delivered)
        {
            request.CompletedDate ??= DateTime.Now;
        }

        dbContext.SaveChanges();
    }

    public void AddOperation(int requestId, string description, decimal cost)
    {
        if (requestId <= 0)
        {
            throw new InvalidOperationException("Select a service request first.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException("Operation description is required.");
        }

        if (cost < 0)
        {
            throw new InvalidOperationException("Operation cost cannot be negative.");
        }

        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();
        var request = dbContext.ServiceRequests.Find(requestId)
            ?? throw new InvalidOperationException("Service request not found.");

        dbContext.ServiceOperations.Add(new ServiceOperation
        {
            ServiceRequestId = requestId,
            Description = description.Trim(),
            Cost = cost,
            OperationDate = DateTime.Now
        });

        request.Status = ServiceRequestStatus.InProgress;
        dbContext.SaveChanges();
    }

    public void AddPartUsage(int requestId, int sparePartId, int quantity)
    {
        if (requestId <= 0)
        {
            throw new InvalidOperationException("Select a service request first.");
        }

        if (sparePartId <= 0)
        {
            throw new InvalidOperationException("Select a spare part first.");
        }

        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();

        var request = dbContext.ServiceRequests.Find(requestId)
            ?? throw new InvalidOperationException("Service request not found.");

        var sparePart = dbContext.SpareParts.Find(sparePartId)
            ?? throw new InvalidOperationException("Spare part not found.");

        if (sparePart.StockQuantity < quantity)
        {
            throw new InvalidOperationException("Not enough stock for the selected spare part.");
        }

        var existingUsage = dbContext.PartUsages
            .FirstOrDefault(usage => usage.ServiceRequestId == requestId && usage.SparePartId == sparePartId);

        if (existingUsage is null)
        {
            dbContext.PartUsages.Add(new PartUsage
            {
                ServiceRequestId = requestId,
                SparePartId = sparePartId,
                Quantity = quantity,
                UnitPriceSnapshot = sparePart.UnitPrice
            });
        }
        else
        {
            existingUsage.Quantity += quantity;
        }

        sparePart.StockQuantity -= quantity;
        request.Status = ServiceRequestStatus.InProgress;
        dbContext.SaveChanges();
    }

    public ServiceRequestDetailsViewModel GetRequestDetails(int requestId)
    {
        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();

        var request = dbContext.ServiceRequests
            .AsNoTracking()
            .Include(serviceRequest => serviceRequest.Customer)
            .Include(serviceRequest => serviceRequest.Device)
            .Include(serviceRequest => serviceRequest.ServiceOperations)
            .Include(serviceRequest => serviceRequest.PartUsages)
                .ThenInclude(usage => usage.SparePart)
            .SingleOrDefault(serviceRequest => serviceRequest.Id == requestId)
            ?? throw new InvalidOperationException("Service request not found.");

        return new ServiceRequestDetailsViewModel
        {
            Id = request.Id,
            CustomerName = request.Customer?.FullName ?? "Unknown",
            DeviceName = $"{request.Device?.Brand} {request.Device?.Model}".Trim(),
            ProblemDescription = request.ProblemDescription,
            Status = request.Status.ToString(),
            TotalCost = CalculateTotalCost(request),
            Operations = request.ServiceOperations
                .OrderByDescending(operation => operation.OperationDate)
                .Select(operation =>
                    $"{operation.OperationDate:g} - {operation.Description} ({operation.Cost:C})")
                .ToList(),
            PartUsages = request.PartUsages
                .Select(usage =>
                    $"{usage.SparePart?.Name ?? "Unknown"} x{usage.Quantity} ({usage.UnitPriceSnapshot:C} each)")
                .ToList()
        };
    }

    private static decimal CalculateTotalCost(ServiceRequest request)
    {
        var operationsTotal = request.ServiceOperations.Sum(operation => operation.Cost);
        var partTotal = request.PartUsages.Sum(usage => usage.Quantity * usage.UnitPriceSnapshot);
        return request.LaborCost + operationsTotal + partTotal;
    }
}
