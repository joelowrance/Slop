<!-- 242b33f7-a20d-4c0e-9264-98dc631c1f83 11e9d164-85a1-4210-961c-8eca8175462b -->
# CoreAPI Project Setup with Aspire Integration

## Implementation Steps

### 1. Create CoreAPI Project Files

Create the foundational project structure:

- `VerdaVidaLawnCare.CoreAPI.csproj` - Project file with package references for:
- Npgsql.EntityFrameworkCore.PostgreSQL
- Npgsql.OpenTelemetry
- MassTransit
- MassTransit.RabbitMQ
- OpenTelemetry packages (AspNetCore, OTLP exporter)
- Serilog.AspNetCore
- VerdaVida.Shared project reference

- `Program.cs` - Minimal API setup with:
- Serilog configuration (Console sink)
- OpenTelemetry tracing (HTTP, ASP.NET Core, Npgsql instrumentation)
- PostgreSQL connection via Aspire service defaults
- MassTransit with RabbitMQ configuration
- Health checks
- ProblemDetails middleware

- `appsettings.json` and `appsettings.Development.json` - Configuration files

**Key Files:**

- `src/VerdaVidaLawnCare.CoreAPI/VerdaVidaLawnCare.CoreAPI.csproj`
- `src/VerdaVidaLawnCare.CoreAPI/Program.cs`
- `src/VerdaVidaLawnCare.CoreAPI/appsettings.json`
- `src/VerdaVidaLawnCare.CoreAPI/appsettings.Development.json`

### 2. Add CoreAPI to Solution and AppHost

- Add CoreAPI project to `VerdaVidaLawnCare.sln`
- Update `AppHost.cs` to uncomment and configure CoreAPI with references to postgres and rabbitmq containers
- Verify project builds and runs with AppHost

**Key Files:**

- `VerdaVidaLawnCare.sln`
- `src/VerdaVidaLawnCare.AppHost/AppHost.cs` (lines 25-27)

### 3. Create Basic DbContext

Create a minimal ApplicationDbContext to verify PostgreSQL integration:

- `Data/ApplicationDbContext.cs` - DbContext with PostgreSQL configuration
- Configure connection string injection from Aspire
- Add DbContext to DI container

**Key Files:**

- `src/VerdaVidaLawnCare.CoreAPI/Data/ApplicationDbContext.cs`
- Update `src/VerdaVidaLawnCare.CoreAPI/Program.cs`

### 4. Add Health Check Endpoint

Create a simple health check endpoint to verify all integrations:

- GET `/health` - Returns health status
- GET `/api/test` - Simple test endpoint that logs and traces

**Key Files:**

- Update `src/VerdaVidaLawnCare.CoreAPI/Program.cs`

## Verification

After completion:

1. Build solution: `dotnet build`
2. Run AppHost: `dotnet run --project src/VerdaVidaLawnCare.AppHost`
3. Verify CoreAPI starts and connects to PostgreSQL and RabbitMQ
4. Check Aspire dashboard shows CoreAPI with telemetry
5. Test endpoints: `curl https://localhost:7001/health` and `curl https://localhost:7001/api/test`
6. Verify structured logs appear in console with Serilog formatting
7. Verify traces appear in Aspire dashboard

## Configuration Notes

- PostgreSQL connection: Managed by Aspire service discovery
- RabbitMQ connection: Managed by Aspire service discovery  
- OpenTelemetry: OTLP exporter configured to send to Aspire dashboard
- Serilog: Console sink with JSON formatting for structured logging
- Ports: CoreAPI will run on https://localhost:7001 (from launchSettings.json)

### To-dos

- [ ] Create CoreAPI csproj, Program.cs, and appsettings files with all package references
- [ ] Add CoreAPI to solution file and update AppHost to reference it
- [ ] Create ApplicationDbContext and configure PostgreSQL integration
- [ ] Add health check and test endpoints to verify integrations
- [ ] Build, run, and test all integrations working together