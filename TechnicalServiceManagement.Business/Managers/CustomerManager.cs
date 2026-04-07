using TechnicalServiceManagement.Data;
using TechnicalServiceManagement.Data.Entities;

namespace TechnicalServiceManagement.Business.Managers;

public sealed class CustomerManager
{
    public IReadOnlyList<Customer> GetCustomers()
    {
        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();
        return dbContext.Customers
            .OrderBy(customer => customer.FullName)
            .ToList();
    }

    public Customer CreateCustomer(string fullName, string phone, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Customer name is required.");
        }

        var normalizedName = fullName.Trim();
        var normalizedPhone = phone.Trim();
        var normalizedEmail = email.Trim();

        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            throw new InvalidOperationException("Phone number is required.");
        }

        if (!normalizedPhone.All(char.IsDigit))
        {
            throw new InvalidOperationException("Phone number can only contain digits.");
        }

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

        return customer;
    }
}
