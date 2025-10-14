using System.Diagnostics;
using MassTransit;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;
using VerdaVida.Shared.EndPoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VerdaVida.Shared.ProjectSetup;
using VerdaVidaLawnCare.CoreAPI.Data;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

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

// FluentValidation registration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Minimal endpoints discovery
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

// Configure Entity Framework with PostgreSQL
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


// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        cfg.ConfigureEndpoints(context);
    });
});

// Add ProblemDetails
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
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

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();


static class TestActivitySource
{
    public static readonly string ActivitySourceName = "DbMigrations";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
