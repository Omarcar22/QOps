# QOps

Quality Engineering & Test Management Platform.

## Current Slice

The first vertical slice manages projects through the ASP.NET Core API and SQL Server:

- `POST /api/projects`
- `GET /api/projects`
- `GET /api/projects/{id}`
- `PUT /api/projects/{id}`
- `DELETE /api/projects/{id}`

## Local Development

Requirements: .NET 10 SDK and Docker Desktop.

```powershell
docker compose up -d sqlserver
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project backend/QOps.API
```

The API is available at `http://localhost:5091` when started with the repository launch settings. The development startup creates the initial database schema automatically. Migrations will replace this during the persistence hardening phase.

Run tests with:

```powershell
dotnet test backend/QOps.slnx
```
