# Technical Service Management

A Windows desktop application for electronic repair shops. It brings customer records, repair requests, service costs, and spare-parts inventory into one workflow.

- Register customers and devices, open repair requests, and update their status.
- Record repair operations and assign spare parts to a request. Assigning parts reduces available stock.
- Review open requests and low-stock items from the dashboard.

## Stack

C#, .NET 9, Windows Forms, Entity Framework Core 9.0.4, and SQLite. The solution separates the UI, business rules, and data access into three projects.

## Run locally

Install the .NET 9 SDK on Windows and run this from the repository root.

```powershell
dotnet run --project .\TechnicalServiceManagement.UI\TechnicalServiceManagement.UI.csproj
```

The app creates `AppData/technical-service-management.db` in its output directory on first launch.

[Project planning](docs/TechnicalServiceManagement_ProjectPlanning.md) · [Use cases](docs/use-case-diagram.svg) · [Form navigation](docs/form-navigation-diagram.svg)
