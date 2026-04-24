using TechnicalServiceManagement.Data;
using TechnicalServiceManagement.Data.Entities;

namespace TechnicalServiceManagement.Business.Managers;

public sealed class CustomerManager
{
    private readonly AuditService _auditService = new();

    public IReadOnlyList<Customer> GetCustomers()
    {
        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();
        return dbContext.Customers
            .OrderBy(customer => customer.FullName)
            .ToList();
    }

    public Customer CreateCustomer(string fullName, string phone, string email)
    {
        var normalizedName = InputSanitizer.ValidateRequired(fullName, "Customer name");
        var normalizedPhone = InputSanitizer.ValidatePhone(phone);
        var normalizedEmail = InputSanitizer.ValidateEmail(email);

        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();

        var exists = dbContext.Customers.Any(customer =>
            customer.FullName == normalizedName &&
            customer.Phone == normalizedPhone);

        if (exists)
        {
            throw new InvalidOperationException("This customer already exists.");
        }

        var customer = new Customer
        {
            FullName = normalizedName,
            Phone = normalizedPhone,
            Email = normalizedEmail,
            CreatedAt = DateTime.Now
        };

        dbContext.Customers.Add(customer);
        dbContext.SaveChanges();

        _auditService.LogAction("Create", "Customer", customer.Id,
            $"Registered customer: {normalizedName} | {normalizedPhone}");

        return customer;
    }
}
