# Soul Fitness API

Soul Fitness is a comprehensive gym management system. This project is the backend API that powers the Soul Fitness ecosystem.

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
- **Task Scheduling**: Hangfire
- **Logging**: Custom Utility logging
- **Export/Import**: ClosedXML, CsvHelper

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Installation

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

### API Documentation

Once the application is running, you can access the Swagger UI at:
`http://localhost:1625/swagger/index.html` (or the configured port)

## Testing

The project includes unit tests for controllers and logic using xUnit.
To run the tests:
```bash
dotnet test
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests.

## License

This project is licensed under the MIT License.
