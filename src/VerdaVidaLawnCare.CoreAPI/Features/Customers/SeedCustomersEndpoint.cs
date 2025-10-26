using Microsoft.AspNetCore.Mvc;
using VerdaVida.Shared.EndPoints;
using VerdaVidaLawnCare.CoreAPI.Data;

namespace VerdaVidaLawnCare.CoreAPI.Features.Customers;

/// <summary>
/// Endpoint for seeding customer data
/// </summary>
public class SeedCustomersEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the customer seeding endpoint
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/customers/seed", async (CustomerDataSeeder seeder, ILogger<SeedCustomersEndpoint> logger, int? count = null) =>
        {
            try
            {
                var seedCount = count ?? 1000;
                logger.LogInformation("Received request to seed {Count} customers", seedCount);

                await seeder.SeedCustomersAsync(seedCount);

                var response = new
                {
                    Message = $"Successfully seeded {seedCount} customer records",
                    Count = seedCount,
                    Timestamp = DateTimeOffset.UtcNow
                };

                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Seeding failed: {Message}", ex.Message);
                return Results.Problem(
                    title: "Cannot seed customers",
                    detail: ex.Message,
                    statusCode: 409
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding customers");
                return Results.Problem(
                    title: "An error occurred while seeding customers",
                    detail: "Please check server logs for more details",
                    statusCode: 500
                );
            }
        })
        .WithName("SeedCustomers")
        .WithSummary("Seed customer database")
        .WithDescription("Seeds the database with fake customer data. Only works if the Customers table is empty. Use the count query parameter to specify how many records to generate (default: 1000).")
        .WithTags("Customers", "Seed")
        .Produces<object>(200)
        .Produces<ProblemDetails>(400)
        .Produces<ProblemDetails>(409)
        .Produces<ProblemDetails>(500);
    }
}
