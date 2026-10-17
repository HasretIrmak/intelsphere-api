# IntelSphere API

> Threat Intelligence & Data Breach Monitoring REST API built with .NET 8 and Entity Framework Core.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF_Core-512BD4?style=flat&logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat&logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![OpenAPI](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?style=flat&logo=swagger&logoColor=black)](https://swagger.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

IntelSphere is a RESTful backend for monitoring data breach sources and indexing domain risk. It is built with ASP.NET Core on .NET 8 using a layered structure (controllers, DTOs, and an EF Core data layer) and ships with interactive Swagger / OpenAPI documentation.

## Highlights

- **RESTful Web API** for breach source monitoring and domain risk indexing.
- **Layered structure:** controllers, DTOs for request and response contracts, and persistence isolated in an Entity Framework Core `DbContext`.
- **SQLite persistence** through EF Core, so the API runs locally with no database server to install.
- **Database migrations** managed with EF Core.
- **Interactive API documentation** through Swagger UI (OpenAPI).

## Tech Stack

| Layer | Technology |
| :--- | :--- |
| Language / Runtime | C#, .NET 8 |
| Web framework | ASP.NET Core Web API |
| Data access | Entity Framework Core (code-first migrations) |
| Database | SQLite |
| API documentation | Swagger / OpenAPI |

## Repository Layout

```text
intelsphere-api/
├── IntelSphere.API/        # ASP.NET Core Web API project
├── IntelSphere.API.sln     # Visual Studio solution
└── .gitignore
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- EF Core CLI tools:

```bash
  dotnet tool install --global dotnet-ef
```

### Run Locally

```bash
# 1. Clone the repository
git clone https://github.com/HasretIrmak/intelsphere-api.git
cd intelsphere-api

# 2. Restore dependencies
dotnet restore

# 3. Apply database migrations (creates the local SQLite file)
dotnet ef database update --project IntelSphere.API

# 4. Start the API
dotnet run --project IntelSphere.API
```

When the application starts, the console prints the local URL it is listening on. Swagger UI is enabled in the `Development` environment, which is the default when running with `dotnet run`: open the printed URL with `/swagger` appended to explore and test the endpoints.

## Configuration

The database connection is read from the `DefaultConnection` entry in `IntelSphere.API/appsettings.json`. By default it points to a local SQLite file named `IntelSphere.db`, which is created on first run and excluded from version control.

## Roadmap

- [ ] PostgreSQL support for deployment
- [ ] Automated unit and integration tests
- [ ] GitHub Actions workflow (build and test on every push)
- [ ] Docker and Docker Compose support
- [ ] Authentication and authorization

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
