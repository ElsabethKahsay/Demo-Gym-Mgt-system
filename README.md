# Soul Fitness API

Soul Fitness is a comprehensive gym management system. This project is the backend API that powers the Soul Fitness ecosystem.

> **Project Note**: This system has been successfully hosted and in active production use within a major airline's corporate gym environment since 2023. This repository represents a modernized and refactored version of that battle-tested codebase.

## Features

- **Coaches Management**: Track coach details, attendance, and schedules.
- **Employee Management**: Manage staff information, schedules, and gym attendance.
- **Gym Attendance**: Real-time RFID-based attendance tracking for both coaches and employees.
- **Locker Management**: Assign and track lockers for gym users.
- **Portal Management**: Manage news and feeds for the member portal.
- **Reports**: Generate and view dashboard statistics and detailed logs.
- **User Management**: Authentication, authorization, and role management.
- **FAQ System**: Manage Frequently Asked Questions for easy access by members.
- **Misconduct Logs**: Track and manage misconduct reports.

## Technology Stack

- **Framework**: .NET 9.0 (ASP.NET Core Web API)
- **Database**: SQL Server with Entity Framework Core
- **Documentation**: Swagger UI
- **Logging**: Custom Utility logging
- **Export/Import**: ClosedXML, CsvHelper
- **Containerization**: Docker & Docker Compose
- **CI/CD**: GitHub Actions

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Optional for containerized run)

### Installation (Local)

1. Clone the repository.
2. Update the connection string in `appsettings.json`.
3. Run migrations and seed the database:
   ```bash
   dotnet ef database update --project SoulFitness.Web
   ```
4. Run the application:
   ```bash
   dotnet run --project SoulFitness.Web
   ```

### Installation (Docker)

To run the entire ecosystem (API + SQL Server):
```bash
docker-compose up --build
```
The API will be available at `http://localhost:1625/swagger`.

### API Documentation

Once the application is running, you can access the Swagger UI at:
`http://localhost:1625/swagger/index.html`

## Testing

The project includes unit tests for controllers and logic using xUnit.
To run the tests:
```bash
dotnet test
```

## Postman Collection

A Postman collection is included in the root directory: `SoulFitness_API_Collection.json`. Import this into Postman to start testing the endpoints immediately.

## Contributing

Contributions are welcome! Please feel free to submit pull requests.

## License

This project is licensed under the MIT License.
