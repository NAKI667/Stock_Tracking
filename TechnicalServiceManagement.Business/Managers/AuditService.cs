using TechnicalServiceManagement.Data;
using TechnicalServiceManagement.Data.Entities;

namespace TechnicalServiceManagement.Business.Managers;

/// <summary>
/// Records all business operations to the AuditLog table for traceability.
/// Every create, update, and delete action is logged with a timestamp,
/// the affected entity, and a human-readable description.
/// </summary>
public sealed class AuditService
{
    public void LogAction(string action, string entityName, int entityId, string details)
    {
        try
        {
            using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();

            var logEntry = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details.Length > 500 ? details[..500] : details,
                Timestamp = DateTime.Now
            };

            dbContext.AuditLogs.Add(logEntry);
            dbContext.SaveChanges();
        }
        catch
        {
            // Audit logging must never cause business operations to fail.
            // In a production environment this would be forwarded to an
            // external monitoring service.
            System.Diagnostics.Debug.WriteLine(
                $"[AuditService] Failed to log: {action} {entityName} #{entityId}");
        }
    }

    /// <summary>
    /// Retrieves the most recent audit log entries for dashboard display.
    /// </summary>
    public IReadOnlyList<AuditLog> GetRecentLogs(int count = 15)
    {
        using var dbContext = TechnicalServiceDbContextFactory.CreateDbContext();
        return dbContext.AuditLogs
            .OrderByDescending(log => log.Timestamp)
            .Take(count)
            .ToList();
    }
}
