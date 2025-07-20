# Journey Sharing Platform

This is a backend web API for a journey sharing platform, developed using ASP.NET Core and Entity Framework Core. It enables users to track, manage, and share their journeys securely, including public shareable links.

## ✅ Features Implemented

- ✅ Journey management (CRUD operations)
- ✅ Journey sharing with public and revocable links
- ✅ User registration and login with JWT authentication
- ✅ Role-based access control
- ✅ Filtering, ordering, and pagination of journey data
- ✅ Middleware for custom exception handling
- ✅ FluentValidation for input validation
- ✅ Unit tests written using xUnit and Moq (58% code coverage)
- ✅ Swagger UI for API documentation

##  Planned Features / Improvements

- Serilog structured logging with correlation ID
- Docker support with Dockerfile and `docker-compose.yml`
- Health endpoints: `/healthz` (liveness) and `/readyz` (readiness)
- Secrets configuration via environment variables or secret manager
- Rate-limiting login endpoint (e.g. 5 attempts/min per IP)
- GitHub Actions pipeline:
  - Restore, build, and test backend
  - Code coverage enforcement (≥70%)
  - Roslyn analyzers + StyleCop
  - Docker image build and push
  - Run backend integration tests

## Tech Stack

- .NET 8 / ASP.NET Core
- Entity Framework Core
- xUnit + Moq (unit testing)
- Swagger / Swashbuckle
- SQL Server (or any configured database)
- GitHub for version control

##  Project Structure
/Application → Business logic, services, validators
/Domain → Entities and interfaces
/Infrastructure → EF Core, DbContext, repositories
/Presentation → Web API controllers, middleware, program startup
/Tests → Unit tests using xUnit and Moq
