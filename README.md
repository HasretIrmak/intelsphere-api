# IntelSphere API

> Threat Intelligence & Data Breach Monitoring REST API built with .NET 8 and Entity Framework Core.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF_Core-512BD4?style=flat&logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![OpenAPI](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?style=flat&logo=swagger&logoColor=black)](https://swagger.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

IntelSphere is a RESTful backend for monitoring data breach sources and indexing domain risk. It is built with ASP.NET Core on .NET 8, follows Clean Architecture principles, and ships with interactive Swagger / OpenAPI documentation.

## Highlights

- **RESTful Web API** for breach source monitoring and domain risk indexing.
- **Clean Architecture principles:** decoupled controllers, DTOs for request and response contracts, and persistence isolated behind Entity Framework Core.
- **Automated database migrations** managed with EF Core.
- **Swappable database provider:** SQLite for local development and testing, PostgreSQL for deployment.
- **Interactive API documentation** through Swagger UI (OpenAPI).

## Tech Stack

| Layer | Technology |
| :--- | :--- |
| Language / Runtime | C#, .NET 8 |
| Web framework | ASP.NET Core Web API |
| Data access | Entity Framework Core (code-first migrations) |
| Databases | SQLite (local), PostgreSQL (deployment) |
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

# 3. Apply database migrations
dotnet ef database update --project IntelSphere.API

# 4. Start the API
dotnet run --project IntelSphere.API
```

When the application starts, the console prints the local URL it is listening on. Open that URL with `/swagger` appended to explore and test the endpoints in Swagger UI.

## Design Notes

- **Separation of concerns:** controllers only handle HTTP concerns; data shapes exposed to clients are DTOs rather than database entities.
- **Provider abstraction:** the database provider is chosen through configuration, so the same code runs on SQLite locally and PostgreSQL in deployment.
- **Reproducible schema:** the database schema is versioned through EF Core migrations.

## Roadmap

- [ ] Automated unit and integration tests
- [ ] GitHub Actions workflow (build and test on every push)
- [ ] Docker and Docker Compose support
- [ ] Authentication and authorization

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
