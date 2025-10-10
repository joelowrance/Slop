# Task: AppHost Aspire Setup with OpenTelemetry and Logging

## Commit 1: chore: initialize solution structure [docs/tasks/2025-10-08-apphost-setup.md]
**Description:**
Create basic .NET solution structure following workspace rules: src/ folder for projects, tests/ folder for test projects, directory.build.props, directory.Packages.props, and global.json files. Initialize empty solution file.

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet sln list`
   * **Expected Outcome:** `No projects found in solution` (empty solution)
2. **Logging Check:**
   * **Action:** `ls -la` to verify directory structure
   * **Expected Log:** `src/`, `tests/`, `directory.build.props`, `directory.Packages.props`, `global.json` files present
   * **Toggle Mechanism:** N/A (structure verification)

---

## Commit 2: feat: create AppHost project with Aspire dependencies [docs/tasks/2025-10-08-apphost-setup.md]
**Description:**
Create AppHost project in src/VerdaVidaLawnCare.AppHost/ using `dotnet new aspire-apphost` template. Configure project file with necessary Aspire packages and dependencies for orchestration.

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet build src/VerdaVidaLawnCare.AppHost/`
   * **Expected Outcome:** `Build succeeded` with no errors
2. **Logging Check:**
   * **Action:** `dotnet run --project src/VerdaVidaLawnCare.AppHost/` (should start dashboard)
   * **Expected Log:** `Now listening on: https://localhost:15000` (Aspire dashboard)
   * **Toggle Mechanism:** N/A (project creation verification)

---

## Commit 3: feat: configure OpenTelemetry in AppHost [docs/tasks/2025-10-08-apphost-setup.md]
**Description:**
Add OpenTelemetry configuration to AppHost Program.cs: configure tracing with Jaeger/OTLP exporter, metrics collection, and service discovery. Include Microsoft.Extensions.Hosting.OpenTelemetry and Aspire telemetry packages.

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test tests/VerdaVidaLawnCare.AppHost.Tests/`
   * **Expected Outcome:** `OpenTelemetry configuration test passes` (custom test for telemetry setup)
2. **Logging Check:**
   * **Action:** `dotnet run --project src/VerdaVidaLawnCare.AppHost/` and check dashboard
   * **Expected Log:** `OpenTelemetry traces visible in Aspire dashboard`
   * **Toggle Mechanism:** `appsettings.json: "OpenTelemetry:Enabled": true`

---

## Commit 4: feat: configure structured logging with Serilog [docs/tasks/2025-10-08-apphost-setup.md]
**Description:**
Configure structured logging using Serilog with JSON output format, console sink, and file sink. Add Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File packages. Configure in Program.cs with proper log levels and formatting.

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test --filter LoggingConfigurationTests`
   * **Expected Outcome:** `Structured log output verification passes`
2. **Logging Check:**
   * **Action:** `dotnet run --project src/VerdaVidaLawnCare.AppHost/` and trigger log events
   * **Expected Log:** `{"Timestamp":"2025-10-08T20:29:00.000Z","Level":"Information","Message":"Application started","Service":"AppHost"}`
   * **Toggle Mechanism:** `appsettings.json: "Serilog:MinimumLevel:Default": "Information"`

---

## Commit 5: feat: create sample service project [docs/tasks/2025-10-08-apphost-setup.md]
**Description:**
Create sample API service project in src/VerdaVidaLawnCare.ApiService/ using `dotnet new webapi` template. Configure it to be discovered by AppHost, add health checks, and demonstrate telemetry integration.

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test tests/VerdaVidaLawnCare.ApiService.Tests/`
   * **Expected Outcome:** `API service health check test passes`
2. **Logging Check:**
   * **Action:** `dotnet run --project src/VerdaVidaLawnCare.AppHost/` and call API endpoints
   * **Expected Log:** `{"Level":"Information","Message":"API request processed","Endpoint":"/health","Duration":"15ms"}`
   * **Toggle Mechanism:** `appsettings.json: "Logging:LogLevel:VerdaVidaLawnCare.ApiService": "Information"`

---

## Commit 6: test: create integration tests for AppHost setup [docs/tasks/2025-10-08-apphost-setup.md]
**Description:**
Create comprehensive integration tests in tests/VerdaVidaLawnCare.AppHost.IntegrationTests/ to verify AppHost startup, service discovery, OpenTelemetry configuration, and logging functionality. Use TestContainers for isolated testing.

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test tests/VerdaVidaLawnCare.AppHost.IntegrationTests/`
   * **Expected Outcome:** `All integration tests pass: AppHost starts, services discovered, telemetry works, logging configured`
2. **Logging Check:**
   * **Action:** `dotnet test --logger "console;verbosity=detailed"`
   * **Expected Log:** `{"Level":"Information","Message":"Integration test completed","TestName":"AppHost_ShouldStartSuccessfully","Duration":"2.5s"}`
   * **Toggle Mechanism:** `appsettings.Test.json: "Logging:LogLevel:Default": "Information"`

---

## Commit 7: docs: create setup documentation and README [docs/tasks/2025-10-08-apphost-setup.md]
**Description:**
Create comprehensive documentation: README.md with setup instructions, docs/setup.md with detailed configuration guide, and docs/architecture.md explaining the Aspire setup, OpenTelemetry integration, and logging strategy.

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet build` (verify all projects still build)
   * **Expected Outcome:** `Build succeeded` for all projects
2. **Logging Check:**
   * **Action:** `cat README.md | grep -i "aspire\|opentelemetry\|logging"`
   * **Expected Log:** `Documentation contains references to Aspire, OpenTelemetry, and logging setup`
   * **Toggle Mechanism:** N/A (documentation verification)

---

## Commit 8: chore: finalize solution configuration [docs/tasks/2025-10-08-apphost-setup.md]
**Description:**
Finalize solution configuration: add all projects to solution file, configure build order, set up launch profiles for debugging, and ensure all projects reference correct packages. Add .gitignore for .NET projects.

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet sln list` and `dotnet build`
   * **Expected Outcome:** `All projects listed in solution and build successfully`
2. **Logging Check:**
   * **Action:** `dotnet run --project src/VerdaVidaLawnCare.AppHost/` and verify dashboard shows all services
   * **Expected Log:** `Aspire dashboard shows AppHost and ApiService running with telemetry data`
   * **Toggle Mechanism:** N/A (final verification)
