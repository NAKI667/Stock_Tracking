using TechnicalServiceManagement.Business;

namespace TechnicalServiceManagement.UI;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var databasePath = Path.Combine(
            AppContext.BaseDirectory,
            "AppData",
            "technical-service-management.db");

        ApplicationBootstrapper.InitializeDatabase(databasePath);

        Application.Run(new DashboardForm());
    }
}
