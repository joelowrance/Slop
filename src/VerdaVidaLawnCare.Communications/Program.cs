using MassTransit;
using Serilog;
using VerdaVida.Shared.ProjectSetup;
using VerdaVidaLawnCare.Communications.Consumers;
using VerdaVidaLawnCare.Communications.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddCors();
builder.Services.AddOpenApi();

// Register email and template services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILiquidTemplateService, LiquidTemplateService>();

// Register weather service
// Service discovery will automatically resolve "WeatherApp" when running in Aspire
// Fallback to localhost for standalone development
builder.Services.AddHttpClient<IWeatherService, WeatherService>((_, client) =>
{
    // Aspire service discovery will resolve "WeatherApp" automatically via AddServiceDefaults()
    // For standalone development, fallback to localhost
    client.BaseAddress = new Uri("http://WeatherApp");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Only configure MassTransit when running in Aspire (when RabbitMQ connection string is available)
if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("rabbitmq")))
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<CustomerCreatedEventConsumer>();
        x.AddConsumer<EstimateSentEventConsumer>();
        x.SetKebabCaseEndpointNameFormatter();
        x.UsingRabbitMq((context, configuration) =>
        {
            configuration.Host(builder.Configuration.GetConnectionString("rabbitmq"));
            configuration.ConfigureEndpoints(context);
        });
    });
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
app.MapPrometheusScrapingEndpoint();

// Add test endpoint
app.MapGet("/api/test", (ILogger<Program> logger) =>
{
    logger.LogInformation("Communications service test endpoint called at {Timestamp}", DateTime.UtcNow);

    var result = new
    {
        Message = "Communications service is working!",
        Timestamp = DateTime.UtcNow,
        Service = "VerdaVidaLawnCare.Communications",
        Version = "1.0.0"
    };

    logger.LogInformation("Communications service test endpoint completed successfully");

    return Results.Ok(result);
})
.WithName("TestEndpoint")
.WithOpenApi();

app.Run();
