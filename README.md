# SimpleFlow

Small ERP application built with ASP.NET Core MVC, Razor views, Entity Framework Core, and SQL Server.

## Current modules

- Customers
- Products
- Warehouses

## Run locally

Copy `SimpleFlow/appsettings.Local.example.json` to `SimpleFlow/appsettings.Local.json`,
set `ConnectionStrings:DefaultConnectionString`, apply the EF Core migrations, and run the web project.
The local settings file is excluded from Git.

```powershell
dotnet ef database update --project SimpleFlow/SimpleFlow.csproj
dotnet run --project SimpleFlow/SimpleFlow.csproj
```
