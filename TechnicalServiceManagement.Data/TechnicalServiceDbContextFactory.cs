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

        // EnsureCreated only creates the DB when it does not exist.
        // If the schema was extended after the initial creation
        // (e.g. AuditLogs table added), it will NOT add missing tables.
        // Calling EnsureDeleted + EnsureCreated is too destructive,
        // so we first try EnsureCreated, then apply missing tables manually.
        dbContext.Database.EnsureCreated();

        EnsureMissingTablesCreated(dbContext);
    }

    /// <summary>
    /// Checks for tables that might be missing in an older database and creates them.
    /// This handles the case where new entity types were added after the DB was first created.
    /// </summary>
    private static void EnsureMissingTablesCreated(TechnicalServiceDbContext dbContext)
    {
        try
        {
            // Check if AuditLogs table exists by trying a simple query
            dbContext.Database.ExecuteSqlRaw(
                "CREATE TABLE IF NOT EXISTS AuditLogs (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "Action TEXT NOT NULL DEFAULT '' CHECK(length(Action) <= 50), " +
                "EntityName TEXT NOT NULL DEFAULT '' CHECK(length(EntityName) <= 100), " +
                "EntityId INTEGER NOT NULL DEFAULT 0, " +
                "Details TEXT NOT NULL DEFAULT '' CHECK(length(Details) <= 500), " +
                "Timestamp TEXT NOT NULL DEFAULT '0001-01-01 00:00:00')");

            dbContext.Database.ExecuteSqlRaw(
                "CREATE INDEX IF NOT EXISTS IX_AuditLogs_Timestamp ON AuditLogs (Timestamp)");
        }
        catch
        {
            // If table creation fails, the app can still function
            // without audit logging.
            System.Diagnostics.Debug.WriteLine(
                "[DbFactory] Warning: Could not ensure AuditLogs table exists.");
        }
    }
}
