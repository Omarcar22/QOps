# QOps

QOps is a release operations and QA platform designed to mirror how software teams manage projects, environments, deployments, releases, and validation gates in a real delivery workflow.



## What this project does

QOps helps teams organize and control the software release lifecycle by combining several operational workflows into one system:

- Manage projects and their deployment environments
- Track releases, deployments, and version states
- Define delivery pipelines with reusable execution steps
- Support Build, Deploy, and Playwright validation stages
- Control access through role-based authorization
- Provide visibility into QA evidence and automated test outcomes
- Simulate a real operational dashboard for software delivery teams

This is not just a CRUD app; it is designed to feel like a lightweight internal platform used by engineering and release teams.

## Why this project matters

QOps was built to show that I can work across the full stack and understand how modern software delivery systems are structured in practice.

It demonstrates capability in:

- ASP.NET Core API architecture and service layering
- Clean domain and application separation
- Entity Framework Core and SQL Server persistence
- JWT authentication and authorization policies
- React + TypeScript dashboard design
- Automated testing with xUnit and Playwright
- CI-oriented quality evidence and artifact generation

## Tech stack

- .NET 10
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- React
- TypeScript
- Vite
- JWT authentication
- xUnit
- Playwright
- GitHub Actions

## Architecture overview

The project follows a layered architecture with a clear separation of concerns:

- API layer: backend/QOps.API
- Application layer: backend/QOps.Application
- Domain layer: backend/QOps.Domain
- Infrastructure layer: backend/QOps.Infrastructure
- Frontend: frontend/src
- Automated evidence generation: scripts/
- CI pipeline: .github/workflows/

### Key files

- [backend/QOps.API/Program.cs](backend/QOps.API/Program.cs): API startup, JWT configuration, dependency injection, database setup, and authorization policies.
- [backend/QOps.Application](backend/QOps.Application): business logic for projects, environments, deployments, releases, pipelines, and users.
- [backend/QOps.Infrastructure/Persistence/QOpsDbContext.cs](backend/QOps.Infrastructure/Persistence/QOpsDbContext.cs): EF Core persistence model and entity mapping.
- [frontend/src/App.tsx](frontend/src/App.tsx): main application dashboard, forms, auth flow, role-based views, and product interactions.
- [frontend/playwright.config.ts](frontend/playwright.config.ts): Playwright configuration and browser automation setup.
- [frontend/tests/e2e/qops-smoke.spec.ts](frontend/tests/e2e/qops-smoke.spec.ts): smoke tests that validate the main user journey and permission rules.
- [.github/workflows/playwright-evidence.yml](.github/workflows/playwright-evidence.yml): CI workflow for test execution and evidence artifact publication.
- [scripts/generate-test-evidence.mjs](scripts/generate-test-evidence.mjs): generates QA evidence JSON used as artifact output.

## Local environment configuration

Create a local environment file from the sample template and set your own values before running the project.

```powershell
Copy-Item .env.example .env
```

The sample file contains the required values for the local database, JWT signing key, and related test settings. Never commit your personal .env file.

### Demo accounts for local testing

For a quick local demo login, the application seeds the following demo accounts from the local environment values. These are intended only for local demonstration and portfolio use.

- Admin: testadmin@test.com / Admintest1
- Developer: developer-demo@qops.local / DeveloperDemo123!
- Viewer: viewer-demo@qops.local / ViewerDemo123!

> These values are kept only in the local .env file and in this README for demo purposes. They are not used in production or committed as real secrets.

### Environment variables

The project expects the following values to be available locally in a .env file or active shell environment:

- QOPS_DATABASE_CONNECTION
- QOPS_TEST_DATABASE_CONNECTION
- JWT_KEY
- JWT_ISSUER
- MSSQL_SA_PASSWORD
- E2E_ADMIN_EMAIL
- E2E_ADMIN_PASSWORD
- E2E_DEVELOPER_EMAIL
- E2E_DEVELOPER_PASSWORD
- E2E_VIEWER_EMAIL
- E2E_VIEWER_PASSWORD

A working .env example is provided in [.env.example](.env.example).

## Getting started

### Requirements

- .NET 10 SDK
- Node.js 22+
- Docker Desktop or Docker Engine
- Git

### 1) Start the database

```powershell
docker compose up -d sqlserver
```

### 2) Start the backend

```powershell
dotnet restore backend/QOps.slnx
dotnet run --project backend/QOps.API/QOps.API.csproj
```

The API runs on the default local configuration and uses the configured SQL Server connection from the app settings.

### 3) Start the frontend

```powershell
cd frontend
npm install
npm run dev -- --host 127.0.0.1 --port 4173
```

If port 4173 is already in use, Vite will automatically move to the next available port. In that case, the terminal output will show the actual URL to use.

Then open:

- Frontend: http://127.0.0.1:4173
- API: http://127.0.0.1:5091

If the frontend chooses a fallback port such as 4175, use that exact URL instead.

## Available workflows

After login, the platform supports the following operational flows:

- Create and view projects
- Add environments and deployment targets
- Track deployments by version and status
- Create release records for delivery readiness
- Define pipeline steps such as Build, Deploy, and Playwright validation
- Review role-based access restrictions for viewers and administrators
- Validate automated evidence from QA execution

## Testing

### Backend tests

```powershell
dotnet test tests/QOps.ApiTests/QOps.ApiTests.csproj --nologo
```

### Frontend build

```powershell
cd frontend
npm run build
```

### Playwright smoke tests

```powershell
cd frontend
npx playwright test --config=playwright.config.ts
```

The project also includes evidence generation for QA artifacts to make the validation process visible and reusable for portfolio presentation.

## Portfolio positioning

QOps is a realistic platform inspired by real-world release operations. It showcases how a team can manage software delivery responsibilities across multiple domains: engineering, deployment, environment management, operational governance, and automated validation.

It was built to demonstrate not only application development skills, but also product thinking, system design, automation discipline, and a strong focus on quality engineering.

## Project status

This project is functional, testable, and structured to communicate engineering maturity clearly. It is suitable for showcasing in a portfolio, technical interviews, or as part of a DevOps / platform engineering narrative.

## License

This project is intended for educational and portfolio demonstration purposes.
