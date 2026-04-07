# Technical Service Management with Spare Parts Tracking

## 1. Project Name and Business Description
This project is a Windows desktop application developed for electronic device service businesses. The application helps staff register customers, accept faulty devices, open service requests, track repair operations, manage spare parts usage, and monitor delivery progress. The main business goal is to improve technical service workflow, reduce manual errors, and provide consistent reporting for daily operations.

## 2. Job Workflow Explanation
1. Reception staff registers a new customer or selects an existing customer.
2. Reception staff creates a service request for the customer device.
3. The device brand, model, serial number, and problem description are recorded.
4. The technician reviews the request and adds service operations.
5. The technician assigns spare parts to the request if required.
6. The system automatically reduces spare part stock quantities after assignment.
7. The service request status is updated from `Received` to `In Progress`, `Waiting For Part`, `Completed`, or `Delivered`.
8. The manager monitors open requests, finished requests, and low-stock parts from the dashboard.

## 3. Use Case Diagram
Actors:
- Reception Staff
- Technician
- Manager

Main use cases:
- Register customer
- Create service request
- Update request status
- Add service operation
- Assign spare part
- Track stock quantity
- Review dashboard summary

Attached diagram:
- `docs/use-case-diagram.svg`

## 4. Three Layer Architecture

### 4.1 User Interface Layer
The user interface is implemented as a WinForms desktop application and contains the following active forms:
- `DashboardForm`: shows summary cards and recent service requests
- `CustomerForm`: adds and lists customers
- `ServiceRequestForm`: creates requests, updates status, adds operations, and assigns spare parts
- `SparePartForm`: adds stock and lists current inventory

Page and form transitions:
- `DashboardForm -> CustomerForm`
- `DashboardForm -> ServiceRequestForm`
- `DashboardForm -> SparePartForm`
- `ServiceRequestForm` loads customer and spare part data from the business layer

Attached navigation diagram:
- `docs/form-navigation-diagram.svg`

### 4.2 Business Logic Layer
The business layer contains the project rules and service logic:
- `CustomerManager`: validates and stores customer information
- `ServiceRequestManager`: creates service requests, calculates total cost, updates service status, and records operations and part usage
- `SparePartManager`: creates spare parts, refills stock, and provides inventory data
- `ApplicationBootstrapper`: initializes the database for the desktop application

Important business rules:
- Customer name cannot be empty.
- Device brand, model, and problem description are required when creating a request.
- Spare part usage cannot exceed available stock quantity.
- Request status is automatically moved to `In Progress` after operations or part assignments.

### 4.3 Data Layer
The data layer uses Entity Framework Core with SQLite.

Main tables and relationships:
- `Customers`
  - `Id`, `FullName`, `Phone`, `Email`, `CreatedAt`
- `Devices`
  - `Id`, `CustomerId`, `Brand`, `Model`, `SerialNumber`
- `ServiceRequests`
  - `Id`, `CustomerId`, `DeviceId`, `ProblemDescription`, `Notes`, `IntakeDate`, `CompletedDate`, `Status`, `LaborCost`, `IsPaid`
- `ServiceOperations`
  - `Id`, `ServiceRequestId`, `Description`, `OperationDate`, `Cost`
- `SpareParts`
  - `Id`, `Name`, `StockCode`, `UnitPrice`, `StockQuantity`
- `PartUsages`
  - `Id`, `ServiceRequestId`, `SparePartId`, `Quantity`, `UnitPriceSnapshot`

Relationships:
- One customer can have many devices.
- One customer can have many service requests.
- One device can have many service requests.
- One service request can have many service operations.
- One service request can have many part usage records.
- One spare part can be used in many service requests.

## 5. Business Functions
- Register and list customers
- Add or refill spare parts
- Create service requests
- View all service requests on dashboard
- Update request status
- Add service operations with cost
- Assign spare parts to requests
- Track low-stock items

## 6. Database and Collection Usage
The project uses Entity Framework Core and SQLite for permanent storage. In addition, collections are actively used in the application:
- `List<Customer>` for customer listing
- `List<ServiceRequestListItem>` for dashboard and request grids
- `List<string>` for operation history and used parts history
- Entity navigation collections such as `ServiceOperations`, `PartUsages`, and `Devices`

## 7. Development Environment
- IDE: Visual Studio 2022
- Language: C#
- Framework: .NET 8 Windows
- Architecture: 3-layer architecture
- Database: SQLite
- ORM: Entity Framework Core

## 8. Submission Note
This report is written in English as required by the course instructions. The project topic is positioned as a technical service management system, while spare part tracking is handled as a supporting business feature.
