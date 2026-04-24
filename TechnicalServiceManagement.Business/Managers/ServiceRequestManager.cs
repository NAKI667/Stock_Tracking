using Microsoft.EntityFrameworkCore;
using TechnicalServiceManagement.Business.Models;
using TechnicalServiceManagement.Data;
using TechnicalServiceManagement.Data.Entities;

namespace TechnicalServiceManagement.Business.Managers;

public sealed class ServiceRequestManager
{
    private readonly AuditService _auditService = new();

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

        // Bug fix: use database-level counting instead of loading all records into memory
        var queryable = dbContext.ServiceRequests.AsNoTracking();

        return new DashboardSummary
        {
            TotalRequests = queryable.Count(),
            ActiveRequests = queryable.Count(request =>
                request.Status == ServiceRequestStatus.Received
                || request.Status == ServiceRequestStatus.InProgress
                || request.Status == ServiceRequestStatus.WaitingForPart),
            FinishedRequests = queryable.Count(request =>
                request.Status == ServiceRequestStatus.Completed
                || request.Status == ServiceRequestStatus.Delivered),
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
        InputSanitizer.ValidatePositiveInteger(customerId, "Customer selection");

        var normalizedBrand = InputSanitizer.ValidateRequired(brand, "Device brand");
        var normalizedModel = InputSanitizer.ValidateRequired(model, "Device model");
        var normalizedSerial = InputSanitizer.ValidateRequired(serialNumber, "Serial number")
            .ToUpperInvariant();
        var normalizedProblem = InputSanitizer.ValidateRequired(problemDescription, "Problem description");

        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();

        var customer = dbContext.Customers.Find(customerId)
            ?? throw new InvalidOperationException("Selected customer could not be found.");

        var existingDevice = dbContext.Devices.FirstOrDefault(device =>
            device.SerialNumber.ToUpper() == normalizedSerial);

        if (existingDevice is not null && existingDevice.CustomerId != customerId)
        {
            throw new InvalidOperationException("This serial number is already registered for another customer.");
        }

        var device = existingDevice ?? new Device
        {
            CustomerId = customerId,
            Brand = normalizedBrand,
            Model = normalizedModel,
            SerialNumber = normalizedSerial
        };

        if (existingDevice is null)
        {
            dbContext.Devices.Add(device);
        }

        var serviceRequest = new ServiceRequest
        {
            CustomerId = customerId,
            Device = device,
            ProblemDescription = normalizedProblem,
            IntakeDate = DateTime.Now,
            Status = ServiceRequestStatus.Received,
            Notes = $"Accepted by reception for {customer.FullName}.",
            LaborCost = 0m,
            IsPaid = false
        };

        dbContext.ServiceRequests.Add(serviceRequest);
        dbContext.SaveChanges();

        _auditService.LogAction("Create", "ServiceRequest", serviceRequest.Id,
            $"New request for {customer.FullName}: {normalizedBrand} {normalizedModel} ({normalizedSerial})");

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

        var previousStatus = request.Status.ToString();
        request.Status = status;

        if (status is ServiceRequestStatus.Completed or ServiceRequestStatus.Delivered)
        {
            request.CompletedDate ??= DateTime.Now;
        }

        dbContext.SaveChanges();

        _auditService.LogAction("Update", "ServiceRequest", requestId,
            $"Status changed: {previousStatus} -> {status}");
    }

    public void AddOperation(int requestId, string description, decimal cost)
    {
        InputSanitizer.ValidatePositiveInteger(requestId, "Service request selection");
        var normalizedDescription = InputSanitizer.ValidateRequired(description, "Operation description");
        InputSanitizer.ValidateNonNegativeAmount(cost, "Operation cost");

        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();
        var request = dbContext.ServiceRequests.Find(requestId)
            ?? throw new InvalidOperationException("Service request not found.");

        dbContext.ServiceOperations.Add(new ServiceOperation
        {
            ServiceRequestId = requestId,
            Description = normalizedDescription,
            Cost = cost,
            OperationDate = DateTime.Now
        });

        request.Status = ServiceRequestStatus.InProgress;
        dbContext.SaveChanges();

        _auditService.LogAction("Create", "ServiceOperation", requestId,
            $"Operation added: {normalizedDescription} (cost: {cost:C})");
    }

    public void AddPartUsage(int requestId, int sparePartId, int quantity)
    {
        InputSanitizer.ValidatePositiveInteger(requestId, "Service request selection");
        InputSanitizer.ValidatePositiveInteger(sparePartId, "Spare part selection");
        InputSanitizer.ValidatePositiveInteger(quantity, "Quantity");

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

        _auditService.LogAction("Create", "PartUsage", requestId,
            $"Part assigned: {sparePart.Name} x{quantity} (stock remaining: {sparePart.StockQuantity})");
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
