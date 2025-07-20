
# Journey Sharing Platform

This project is a backend web API for a journey sharing platform, developed using ASP.NET Core and Entity Framework Core.

## Features

- Journey management with sharing and public links
- User registration and authentication (with role-based access control)
- Filtering, ordering, and paging for journey data
- Secure public link generation and revocation
- FluentValidation for robust data validation
- Custom exception handling middleware
- JWT-based authentication
- Unit testing using xUnit and Moq
- Swagger UI for API documentation

## Project Structure

```
/Application
/Domain
/Infrastructure
/WebAPI
```

## Tech Stack

- .NET 7 / ASP.NET Core
- Entity Framework Core
- JWT Authentication
- xUnit + Moq for unit testing
- Swagger for API documentation

## How to Run

1. Clone the repository
```bash
git clone https://github.com/your-username/journey-sharing-backend.git
cd journey-sharing-backend
```

2. Setup the database (SQL Server or your preferred DB)
3. Run the project using Visual Studio or CLI:
```bash
dotnet build
dotnet ef database update
dotnet run --project WebAPI
```

4. Open Swagger UI:
```
https://localhost:{port}/swagger/index.html
```

## Test

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Ensure coverage is ≥ 70%.

## Future Improvements

- Docker support
- GitHub Actions CI/CD pipeline
- Serilog for logging (optional)
- Redis/Memcached for caching

## License

MIT
