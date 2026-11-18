# SmartParking

A Smart Parking Management System developed with C# and .NET 10, featuring a console application and an interactive web dashboard.

## About the Developer

**Amirhossein Sarinia**

Software Developer focused on C#, .NET, Object-Oriented Programming, and practical software engineering.

This project was designed and developed by Amirhossein Sarinia to demonstrate the application of object-oriented programming principles and modern .NET technologies in a real-world parking management system.

The implementation emphasizes structured code, reusable components, separation of responsibilities, and practical business logic.

GitHub: [@amirhossein-isarinia](https://github.com/amirhossein-isarinia)

## Project Overview

SmartParking provides two applications that use the same core parking management logic:

- **Console Application:** An interactive terminal-based parking management system.
- **Web Dashboard:** An ASP.NET Core and Blazor Interactive Server application for managing and monitoring parking operations.

## Features

- Support for cars, motorcycles, trucks, buses, and electric cars.
- Vehicle entry and exit registration.
- License plate validation and normalization.
- Parking capacity management.
- Duplicate license plate prevention.
- VIP discount calculation.
- Vehicle-specific parking fees.
- Automatic billable parking-hour calculation.
- Parking records and exit history.
- Revenue and financial reports.
- Dashboard analytics and management pages.
- Interactive console interface.
- Interactive web dashboard.

## Technologies

- C#
- .NET 10
- ASP.NET Core
- Blazor Interactive Server
- LINQ
- Object-Oriented Programming

## Project Structure

```text
SmartParking/
├── SmartParking.csproj
├── Program.cs
├── Models/
├── Services/
├── Docs/
├── SmartParking.Dashboard/
│   ├── SmartParking.Dashboard.csproj
│   └── ...
└── README.md
```

## Getting Started

### Requirements

- .NET 10 SDK

### Run the Console Application

```bash
dotnet run --project SmartParking.csproj
```

### Run the Web Dashboard

```bash
dotnet run --project SmartParking.Dashboard/SmartParking.Dashboard.csproj --urls http://localhost:8080
```

Then open:

http://localhost:8080

## Architecture and Data

The console application and web dashboard are separate application processes that share the project's core business logic.

Parking data is stored in application memory. It is not persisted after the application process is restarted, and separate application processes do not share their runtime parking state.

## Documentation

See the `Docs` directory for the project's additional documentation, including conceptual explanations and the UML class diagram.

## License

No open-source license has been specified yet.
