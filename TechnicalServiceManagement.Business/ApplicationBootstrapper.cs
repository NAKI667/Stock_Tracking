using TechnicalServiceManagement.Data;

namespace TechnicalServiceManagement.Business;

public static class ApplicationBootstrapper
{
    public static void InitializeDatabase(string databasePath)
    {
        TechnicalServiceDbContextFactory.Configure(databasePath);
        TechnicalServiceDbContextFactory.InitializeDatabase();
    }
}
