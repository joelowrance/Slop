# Task: AppHost Aspire Setup with PostgreSQL, RabbitMQ, and SMTP Server

## Overview
This plan extends the existing Aspire setup to include essential infrastructure services: PostgreSQL database, RabbitMQ message broker with management UI, and a fake SMTP server for email testing. This provides a complete development environment for the VerdaVidaLawnCare microservices architecture.

## Architecture Components
- **AppHost**: Orchestrates all services and provides unified dashboard
- **PostgreSQL**: Primary database for CoreAPI and Equipment services
- **RabbitMQ**: Message broker for inter-service communication
- **MailHog**: Fake SMTP server for email testing in Communications service
- **OpenTelemetry**: Distributed tracing and metrics collection
- **Serilog**: Structured logging across all services

---

## Commit 1: feat: create AppHost project with infrastructure services [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Create AppHost project in `src/VerdaVidaLawnCare.AppHost/` using `dotnet new aspire-apphost` template. Configure PostgreSQL, RabbitMQ with management UI, and MailHog SMTP server containers. Set up proper networking and service discovery.

**Implementation Details:**
- Create AppHost project with Aspire orchestration
- Configure PostgreSQL container with persistent volume
- Configure RabbitMQ container with management plugin
- Configure MailHog container for SMTP testing
- Set up service-to-service communication
- Configure health checks for all services

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet build src/VerdaVidaLawnCare.AppHost/`
   * **Expected Outcome:** `Build succeeded` with no errors
2. **Logging Check:**
   * **Action:** `dotnet run --project src/VerdaVidaLawnCare.AppHost/`
   * **Expected Log:** `Now listening on: https://localhost:15000` (Aspire dashboard)
   * **Expected Services:** PostgreSQL, RabbitMQ, MailHog containers running
   * **Toggle Mechanism:** N/A (project creation verification)

---

## Commit 2: feat: configure PostgreSQL with Entity Framework integration [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Configure PostgreSQL container with proper connection strings, database initialization, and Entity Framework Core integration. Set up connection string management and database health checks.

**Implementation Details:**
- Configure PostgreSQL container with persistent storage
- Set up connection string configuration
- Configure Entity Framework Core with PostgreSQL
- Add database health checks
- Set up connection string injection for services
- Configure database initialization scripts

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test tests/VerdaVidaLawnCare.AppHost.Tests/ --filter DatabaseConfigurationTests`
   * **Expected Outcome:** `PostgreSQL connection and health check tests pass`
2. **Logging Check:**
   * **Action:** Check Aspire dashboard for PostgreSQL service
   * **Expected Log:** `PostgreSQL service healthy and accessible on port 5432`
   * **Toggle Mechanism:** `appsettings.json: "PostgreSQL:Enabled": true`

---

## Commit 3: feat: configure RabbitMQ with management UI and message routing [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Configure RabbitMQ container with management UI, message queues, exchanges, and routing. Set up proper authentication and connection management for inter-service communication.

**Implementation Details:**
- Configure RabbitMQ container with management plugin
- Set up default exchanges and queues
- Configure connection strings for services
- Add RabbitMQ health checks
- Set up message routing patterns
- Configure management UI access

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test tests/VerdaVidaLawnCare.AppHost.Tests/ --filter RabbitMQConfigurationTests`
   * **Expected Outcome:** `RabbitMQ connection and queue creation tests pass`
2. **Logging Check:**
   * **Action:** Access RabbitMQ management UI at `http://localhost:15672`
   * **Expected Log:** `RabbitMQ management UI accessible with default credentials`
   * **Toggle Mechanism:** `appsettings.json: "RabbitMQ:ManagementUI:Enabled": true`

---

## Commit 4: feat: configure MailHog SMTP server for email testing [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Configure MailHog container as a fake SMTP server for email testing. Set up SMTP configuration and web UI for viewing sent emails during development.

**Implementation Details:**
- Configure MailHog container with SMTP and web UI
- Set up SMTP connection configuration
- Configure email testing endpoints
- Add MailHog health checks
- Set up web UI access for email inspection
- Configure SMTP settings for Communications service

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test tests/VerdaVidaLawnCare.AppHost.Tests/ --filter SMTPConfigurationTests`
   * **Expected Outcome:** `MailHog SMTP server and web UI tests pass`
2. **Logging Check:**
   * **Action:** Access MailHog web UI at `http://localhost:8025`
   * **Expected Log:** `MailHog web UI accessible for email testing`
   * **Toggle Mechanism:** `appsettings.json: "MailHog:Enabled": true`

---

## Commit 5: feat: integrate services with existing CoreAPI, Communications, and Equipment projects [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Integrate the infrastructure services with existing VerdaVidaLawnCare projects. Configure each service to use the appropriate infrastructure (PostgreSQL for CoreAPI/Equipment, RabbitMQ for messaging, MailHog for Communications).

**Implementation Details:**
- Update CoreAPI project to use PostgreSQL connection
- Update Equipment project to use PostgreSQL connection
- Update Communications project to use MailHog SMTP
- Configure RabbitMQ messaging in all services
- Set up service discovery and health checks
- Configure inter-service communication

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test tests/VerdaVidaLawnCare.IntegrationTests/`
   * **Expected Outcome:** `All service integration tests pass`
2. **Logging Check:**
   * **Action:** `dotnet run --project src/VerdaVidaLawnCare.AppHost/` and verify all services
   * **Expected Log:** `All services (CoreAPI, Communications, Equipment) connected to infrastructure`
   * **Toggle Mechanism:** `appsettings.json: "Services:IntegrationEnabled": true`

---

## Commit 6: feat: configure OpenTelemetry with distributed tracing [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Configure OpenTelemetry for distributed tracing across all services and infrastructure. Set up tracing for database operations, message queuing, and email sending.

**Implementation Details:**
- Configure OpenTelemetry tracing in AppHost
- Add database operation tracing
- Add message queue operation tracing
- Add email operation tracing
- Configure trace correlation across services
- Set up metrics collection for infrastructure

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test tests/VerdaVidaLawnCare.AppHost.Tests/ --filter OpenTelemetryTests`
   * **Expected Outcome:** `OpenTelemetry configuration and tracing tests pass`
2. **Logging Check:**
   * **Action:** Check Aspire dashboard for trace data
   * **Expected Log:** `Distributed traces visible across all services and infrastructure`
   * **Toggle Mechanism:** `appsettings.json: "OpenTelemetry:Enabled": true`

---

## Commit 7: feat: configure structured logging with Serilog across all services [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Configure structured logging using Serilog across all services and infrastructure. Set up centralized logging with proper correlation IDs and service identification.

**Implementation Details:**
- Configure Serilog in AppHost and all services
- Set up structured logging with JSON format
- Add correlation ID tracking
- Configure log levels per service
- Set up centralized log aggregation
- Add infrastructure operation logging

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test --filter LoggingConfigurationTests`
   * **Expected Outcome:** `Structured logging configuration tests pass`
2. **Logging Check:**
   * **Action:** Trigger operations across all services
   * **Expected Log:** `{"Timestamp":"2025-10-08T20:29:00.000Z","Level":"Information","Message":"Service operation completed","Service":"CoreAPI","CorrelationId":"abc123"}`
   * **Toggle Mechanism:** `appsettings.json: "Serilog:MinimumLevel:Default": "Information"`

---

## Commit 8: test: create comprehensive integration tests for infrastructure [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Create comprehensive integration tests to verify all infrastructure services work together. Test database operations, message queuing, email sending, and service communication.

**Implementation Details:**
- Create integration tests for PostgreSQL operations
- Create integration tests for RabbitMQ messaging
- Create integration tests for MailHog email sending
- Create end-to-end service communication tests
- Use TestContainers for isolated testing
- Add performance and load testing

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet test tests/VerdaVidaLawnCare.AppHost.IntegrationTests/`
   * **Expected Outcome:** `All infrastructure integration tests pass`
2. **Logging Check:**
   * **Action:** `dotnet test --logger "console;verbosity=detailed"`
   * **Expected Log:** `{"Level":"Information","Message":"Integration test completed","TestName":"Infrastructure_ShouldWorkTogether","Duration":"5.2s"}`
   * **Toggle Mechanism:** `appsettings.Test.json: "Logging:LogLevel:Default": "Information"`

---

## Commit 9: docs: create comprehensive setup documentation [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Create comprehensive documentation for the Aspire setup including infrastructure services, configuration, troubleshooting, and development workflows.

**Implementation Details:**
- Create README.md with complete setup instructions
- Create infrastructure configuration guide
- Create service integration documentation
- Create troubleshooting guide
- Create development workflow documentation
- Add architecture diagrams

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet build` (verify all projects still build)
   * **Expected Outcome:** `Build succeeded` for all projects
2. **Logging Check:**
   * **Action:** `cat README.md | grep -i "aspire\|postgresql\|rabbitmq\|mailhog"`
   * **Expected Log:** `Documentation contains references to all infrastructure services`
   * **Toggle Mechanism:** N/A (documentation verification)

---

## Commit 10: chore: finalize solution configuration and launch profiles [docs/features/2025-10-08-20-29-apphost-aspire-setup.md]
**Description:**
Finalize solution configuration with all projects, proper build order, launch profiles for debugging, and ensure all infrastructure services are properly configured and accessible.

**Implementation Details:**
- Add all projects to solution file
- Configure build dependencies
- Set up launch profiles for debugging
- Configure service startup order
- Add .gitignore for .NET and Docker
- Verify all services start correctly

**Verification:**
1. **Automated Test(s):**
   * **Command:** `dotnet sln list` and `dotnet build`
   * **Expected Outcome:** `All projects listed in solution and build successfully`
2. **Logging Check:**
   * **Action:** `dotnet run --project src/VerdaVidaLawnCare.AppHost/` and verify dashboard
   * **Expected Log:** `Aspire dashboard shows all services running with infrastructure healthy`
   * **Toggle Mechanism:** N/A (final verification)

---

## Service Endpoints and Access Points

### Aspire Dashboard
- **URL:** `https://localhost:15000`
- **Purpose:** Centralized monitoring and service management
- **Features:** Service health, logs, traces, metrics

### PostgreSQL Database
- **Host:** `localhost`
- **Port:** `5432`
- **Database:** `verdaVidaLawnCare`
- **Username:** `postgres`
- **Password:** `postgres` (development only)

### RabbitMQ Management UI
- **URL:** `http://localhost:15672`
- **Username:** `guest`
- **Password:** `guest`
- **Purpose:** Message queue management and monitoring

### MailHog SMTP Server
- **SMTP Host:** `localhost`
- **SMTP Port:** `1025`
- **Web UI:** `http://localhost:8025`
- **Purpose:** Email testing and inspection

## Configuration Files

### AppHost Configuration
- `src/VerdaVidaLawnCare.AppHost/Program.cs` - Main orchestration
- `src/VerdaVidaLawnCare.AppHost/appsettings.json` - Service configuration
- `src/VerdaVidaLawnCare.AppHost/appsettings.Development.json` - Development overrides

### Service Configurations
- Connection strings for PostgreSQL
- RabbitMQ connection settings
- MailHog SMTP configuration
- OpenTelemetry and logging settings

## Development Workflow

1. **Start Infrastructure:** `dotnet run --project src/VerdaVidaLawnCare.AppHost/`
2. **Access Dashboard:** Navigate to `https://localhost:15000`
3. **Monitor Services:** Check service health and logs
4. **Test Email:** Send emails and view in MailHog UI
5. **Monitor Messages:** Check RabbitMQ management UI
6. **View Traces:** Use Aspire dashboard for distributed tracing

## Security Considerations

- All services use development credentials (not for production)
- PostgreSQL and RabbitMQ use default credentials
- MailHog is open for email testing
- All services are bound to localhost only
- Consider using secrets management for production deployment