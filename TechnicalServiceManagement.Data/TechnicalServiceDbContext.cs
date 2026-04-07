using Microsoft.EntityFrameworkCore;
using TechnicalServiceManagement.Data.Entities;

namespace TechnicalServiceManagement.Data;

public sealed class TechnicalServiceDbContext(DbContextOptions<TechnicalServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<ServiceOperation> ServiceOperations => Set<ServiceOperation>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<PartUsage> PartUsages => Set<PartUsage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SparePart>()
            .HasIndex(part => part.StockCode)
            .IsUnique();

        modelBuilder.Entity<Device>()
            .HasIndex(device => device.SerialNumber)
            .IsUnique();

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.Status)
            .HasConversion<string>();

        modelBuilder.Entity<SparePart>()
            .Property(part => part.UnitPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.LaborCost)
            .HasPrecision(10, 2);

        modelBuilder.Entity<ServiceOperation>()
            .Property(operation => operation.Cost)
            .HasPrecision(10, 2);

        modelBuilder.Entity<PartUsage>()
            .Property(usage => usage.UnitPriceSnapshot)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Customer>()
            .HasMany(customer => customer.Devices)
            .WithOne(device => device.Customer)
            .HasForeignKey(device => device.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Customer>()
            .HasMany(customer => customer.ServiceRequests)
            .WithOne(request => request.Customer)
            .HasForeignKey(request => request.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Device>()
            .HasMany(device => device.ServiceRequests)
            .WithOne(request => request.Device)
            .HasForeignKey(request => request.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>()
            .HasMany(request => request.ServiceOperations)
            .WithOne(operation => operation.ServiceRequest)
            .HasForeignKey(operation => operation.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServiceRequest>()
            .HasMany(request => request.PartUsages)
            .WithOne(usage => usage.ServiceRequest)
            .HasForeignKey(usage => usage.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SparePart>()
            .HasMany(part => part.PartUsages)
            .WithOne(usage => usage.SparePart)
            .HasForeignKey(usage => usage.SparePartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
