# VerdaVida.Shared

A shared library containing common types, utilities, and extensions used across the VerdaVida Lawn Care application.

## Overview

This library provides foundational components that can be shared between different projects in the VerdaVida solution, including:

- Common exception types
- Result pattern implementation
- Base entity models
- Utility extensions
- Service collection extensions

## Structure

```
VerdaVida.Shared/
├── Common/           # Common types and patterns
│   ├── Result.cs     # Result pattern implementation
│   ├── ValidationException.cs
│   ├── NotFoundException.cs
│   └── UnauthorizedException.cs
├── Models/           # Shared data models
│   ├── PagedResult.cs
│   └── BaseEntity.cs
├── Utilities/        # Utility extensions
│   ├── DateTimeExtensions.cs
│   └── StringExtensions.cs
└── Extensions/       # Service collection extensions
    └── ServiceCollectionExtensions.cs
```

## Key Features

### Result Pattern
The `Result<T>` and `Result` classes provide a consistent way to handle success/failure scenarios without throwing exceptions for business logic errors.

```csharp
// Success case
var result = Result<string>.Success("Operation completed");

// Failure case
var result = Result<string>.Failure("Operation failed");

// Implicit conversions
Result<string> success = "Value";  // Implicit success
Result<string> failure = "Error";  // Implicit failure
```

### Base Entity
All entities should inherit from `BaseEntity` to get common properties like `Id`, `CreatedAt`, `UpdatedAt`, etc.

### Utility Extensions
- **DateTimeExtensions**: UTC conversion, Unix timestamps, start/end of day/week
- **StringExtensions**: Title case, truncation, slug generation, masking

### Exception Types
Specific exception types for different error scenarios:
- `ValidationException`: For validation failures
- `NotFoundException`: For missing resources
- `UnauthorizedException`: For access denied scenarios

## Usage

To use this library in other projects, add a project reference:

```xml
<ProjectReference Include="..\VerdaVida.Shared\VerdaVida.Shared.csproj" />
```

## Dependencies

- .NET 9.0
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options
- System.ComponentModel.Annotations

## Contributing

When adding new shared components:

1. Follow the established folder structure
2. Include comprehensive XML documentation
3. Add appropriate unit tests
4. Follow the coding standards defined in the project rules
5. Use the Result pattern for business logic operations
