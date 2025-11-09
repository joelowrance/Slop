using System.Diagnostics;
using MassTransit;
using FluentValidation;
using VerdaVida.Shared.EndPoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VerdaVida.Shared.EntityFrameworkExtensions;
using VerdaVida.Shared.MediatrPipelines;
using VerdaVida.Shared.OpenTelemetry;
using VerdaVida.Shared.ProjectSetup;
using VerdaVidaLawnCare.CoreAPI.Data;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates;
using VerdaVidaLawnCare.CoreAPI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Configure HttpClient for Python API service
builder.Services.AddHttpClient<IPythonApiService, PythonApiService>((_, client) =>
{
    client.BaseAddress = new Uri("http://WeatherApp");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Configure CORS for React app
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddOpenApi();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(HandlerBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
// FluentValidation registration
//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddProblemDetails();

// Only configure MassTransit when running in Aspire (when RabbitMQ connection string is available)
if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("rabbitmq")))
{
    builder.Services.AddMassTransit(x =>
    {
        //x.AddConsumer<MoveJobToPendingCommandConsumer>(typeof(MoveJobToPendingCommandConsumerDefinition));
        x.SetKebabCaseEndpointNameFormatter();
        x.UsingRabbitMq((context, configuration) =>
        {
            configuration.Host(builder.Configuration.GetConnectionString("rabbitmq"));
            configuration.ConfigureEndpoints(context);
        });
    });
}
// Register MassTransit.Mediator, good luck not confusing this with the other one
builder.Services.AddMediator(cfg =>
{
    cfg.AddConsumers(typeof(Program).Assembly);
});

// Minimal endpoints discovery
builder.Services.AddTransient<IEstimateService, EstimateService>();
builder.Services.AddTransient<CustomerDataSeeder>();

// Python API service is registered via AddHttpClient above
builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("verdevida-connection");
        if (string.IsNullOrEmpty(connectionString))
        {
            // When running outside of Aspire, use a default connection string for development
            connectionString = "Host=localhost;Port=50274;Database=LawnCare;Username=sqluser;Password=sqlpass";
        }

        options.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();
    });

// Only add migration when running in Aspire (when connection string is available)
if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("verdevida-connection")))
{
    builder.Services.AddMigration<ApplicationDbContext, ApplicationDbContextSeeder>();
}




// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.OpenTelemetry()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();



var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();

// Add correlation ID middleware
app.Use(async (context, next) =>
{
    context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId);
    if (string.IsNullOrEmpty(correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
        context.Request.Headers["X-Correlation-ID"] = correlationId;
    }
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    await next();
});

app.UseAuthorization();

// Map discovered endpoints
app.MapEndpoints();

// Add health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});



// Add test endpoint
app.MapGet("/api/test", async (ApplicationDbContext dbContext, ILogger<Program> logger) =>
{
    using var activity =  TestActivitySource.ActivitySource.StartActivity();
    activity?.SetTag("operation", "test");

    logger.LogInformation("Test endpoint called at {Timestamp}", DateTime.UtcNow);

    try
    {
        // Test database connection
        var canConnect = await dbContext.Database.CanConnectAsync();

        var result = new
        {
            Message = "CoreAPI is working!",
            Timestamp = DateTime.UtcNow,
            DatabaseConnected = canConnect,
            Service = "VerdaVidaLawnCare.CoreAPI",
            Version = "1.0.0"
        };

        logger.LogInformation("Test endpoint completed successfully. Database connected: {DatabaseConnected}", canConnect);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error in test endpoint");
        return Results.Problem("Internal server error occurred during test");
    }
})
.WithName("TestEndpoint")
.WithOpenApi();

// Add Python API test endpoint
app.MapGet("/api/python-test", async (IPythonApiService pythonApiService, ILogger<Program> logger) =>
{
    using var activity = TestActivitySource.ActivitySource.StartActivity();
    activity?.SetTag("operation", "python-test");

    logger.LogInformation("Python API test endpoint called at {Timestamp}", DateTime.UtcNow);

    try
    {
        var helloResult = await pythonApiService.GetHelloAsync();
        var pythonVersionResult = await pythonApiService.GetPythonVersionAsync();

        var result = new
        {
            Message = "Python API integration is working!",
            Timestamp = DateTime.UtcNow,
            PythonApiHello = helloResult,
            PythonVersion = pythonVersionResult,
            Service = "VerdaVidaLawnCare.CoreAPI",
            Version = "1.0.0"
        };

        logger.LogInformation("Python API test endpoint completed successfully");

        return Results.Ok(result);
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "HTTP error calling Python API");
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        return Results.Problem(
            detail: "Failed to connect to Python API",
            statusCode: 503,
            title: "Service Unavailable"
        );
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error in Python API test endpoint");
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        return Results.Problem("Internal server error occurred during Python API test");
    }
})
.WithName("PythonApiTestEndpoint")
.WithOpenApi();

// Ensure database is migrated to the latest version
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     await context.Database.MigrateAsync();
// }



app.Run();


static class TestActivitySource
{
    public static readonly string ActivitySourceName = "DbMigrations";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
