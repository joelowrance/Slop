# VerdaVidaLawnCare Project Overview

## Project Description
VerdaVidaLawnCare is a comprehensive lawn care service management application designed to help lawn care businesses manage their operations, customers, and services efficiently.

## Key Features
- Customer management and profiles
- Service scheduling and tracking
- Invoice generation and payment processing
- Employee management and scheduling
- Equipment and inventory tracking
- Reporting and analytics
- Mobile-responsive web interface

## Technology Stack
- **Backend**: .NET 8, ASP.NET Core Web API
- **Frontend**: HTML5, CSS3, JavaScript (ES6+), Bootstrap
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Logging**: Serilog
- **Testing**: xUnit, Moq, FluentAssertions

## Project Structure
```
VerdaVidaLawnCare/
├── src/
│   ├── VerdaVidaLawnCare.API/          # Web API project
│   ├── VerdaVidaLawnCare.Core/         # Business logic and entities
│   ├── VerdaVidaLawnCare.Infrastructure/ # Data access and external services
│   └── VerdaVidaLawnCare.Web/          # Frontend web application
├── tests/
│   ├── VerdaVidaLawnCare.UnitTests/    # Unit tests
│   └── VerdaVidaLawnCare.IntegrationTests/ # Integration tests
├── docs/                               # Documentation
└── .cursorrules/                       # Cursor AI rules and templates
```

## Core Entities
- **Customer**: Customer information and contact details
- **Service**: Lawn care services offered
- **Appointment**: Scheduled service appointments
- **Employee**: Staff members and their roles
- **Invoice**: Billing and payment information
- **Equipment**: Tools and equipment inventory

## Development Guidelines
- Follow clean architecture principles
- Implement proper error handling and logging
- Write comprehensive unit and integration tests
- Use dependency injection for loose coupling
- Follow RESTful API design patterns
- Ensure responsive and accessible UI design

## Security Considerations
- Implement proper authentication and authorization
- Validate all user inputs
- Use HTTPS for all communications
- Follow OWASP security guidelines
- Implement proper data encryption for sensitive information
