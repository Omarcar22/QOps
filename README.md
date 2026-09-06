# QOps

QOps is a portfolio-ready platform for release operations, quality engineering, and deployment governance. The project demonstrates a layered .NET architecture, SQL Server persistence, JWT-based security, automated API validation, and pipeline-driven delivery flows with Playwright-based validation steps.

## What this project demonstrates

- ASP.NET Core Web API with layered architecture
- Entity Framework Core + SQL Server persistence
- JWT authentication and role-based authorization
- Project, environment, deployment, release, and pipeline management
- Pipeline definitions that support Build, Deploy, and Playwright validation stages
- Frontend dashboard built with React + Vite
- Integration tests for API flows and QA-oriented automation workflows

## Portfolio value proposition

This solution is designed to look and behave like a real operational platform for software delivery teams. It combines product management, release control, environment orchestration, and pipeline definition into a single system, which is especially relevant for showcasing engineering skills in backend development, API design, DevOps thinking, and automated quality workflows.

## Core features

- Project lifecycle management
- Environment configuration and deployment tracking
- Release management with versioning and status control
- Pipeline definitions with structured execution steps
- Playwright step support for browser-based validation in delivery pipelines
- Admin user management with roles and permissions

## Tech stack

- .NET 10 + ASP.NET Core Web API
- C# / ASP.NET
- EF Core + SQL Server
- React + TypeScript + Vite
- JWT authentication
- xUnit + ASP.NET Core integration testing
- Playwright-ready pipeline modeling

## Local development

Requirements: .NET 10 SDK and Docker Desktop.

```powershell
docker compose up -d sqlserver
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project backend/QOps.API
```

The API is available at `http://localhost:5091` when started with the repository launch settings. The app uses the `QOpsDev` database and applies pending Entity Framework migrations automatically.

Run the API test suite with:

```powershell
dotnet test tests/QOps.ApiTests/QOps.ApiTests.csproj --nologo
```

Run the frontend build with:

```powershell
cd frontend
npm install
npm run build
```

## Portfolio-ready positioning

QOps represents a realistic operational workflow platform with strong emphasis on pipeline-driven quality gates and release control. It is especially useful to highlight for roles involving backend engineering, platform engineering, delivery automation, and QA tooling strategy.
