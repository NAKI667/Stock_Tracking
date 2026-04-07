using Microsoft.EntityFrameworkCore;

namespace TechnicalServiceManagement.Data;

public static class TechnicalServiceDbContextFactory
{
    private static DbContextOptions<TechnicalServiceDbContext>? _options;
    public static string DatabasePath { get; private set; } = string.Empty;

    public static void Configure(string databasePath)
    {
        DatabasePath = databasePath;

        var builder = new DbContextOptionsBuilder<TechnicalServiceDbContext>();
        builder.UseSqlite($"Data Source={databasePath}");
        _options = builder.Options;
    }

    public static TechnicalServiceDbContext CreateDbContext()
    {
        if (_options is null)
        {
            throw new InvalidOperationException("Database factory is not configured.");
        }

        return new TechnicalServiceDbContext(_options);
    }

    public static void InitializeDatabase()
    {
        if (string.IsNullOrWhiteSpace(DatabasePath))
        {
            throw new InvalidOperationException("Database path is not configured.");
        }

        var directory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var dbContext = CreateDbContext();
        dbContext.Database.EnsureCreated();
    }
}
