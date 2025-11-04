using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerdaVida.Shared.EndPoints;
using VerdaVidaLawnCare.CoreAPI.Data;
using VerdaVidaLawnCare.CoreAPI.Features.Customers.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Customers;

/// <summary>
/// Endpoint for searching customers
/// </summary>
public class SearchCustomersEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the customer search endpoint
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/customers/search", async (IMediator mediator, [FromQuery] string query, [FromQuery] int? maxResults) =>
            {
                var request = new SearchCustomersQuery
                {
                    Query = query ?? string.Empty,
                    MaxResults = maxResults ?? 20
                };
                return await mediator.Send(request);
            })
            .WithName("SearchCustomers")
            .WithSummary("Search customers")
            .WithDescription("Searches for customers by phone, email, or street address. Returns matching active customers.")
            .WithTags("Customers")
            .Produces<CustomerSearchResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }
}

/// <summary>
/// Query for searching customers
/// </summary>
public record SearchCustomersQuery : IRequest<IResult>
{
    public string Query { get; init; } = string.Empty;
    public int MaxResults { get; init; } = 20;
}

/// <summary>
/// Handler for searching customers
/// </summary>
public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, IResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SearchCustomersQueryHandler> _logger;

    public SearchCustomersQueryHandler(ApplicationDbContext context, ILogger<SearchCustomersQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IResult> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Invalid search query",
                    Detail = "Search query cannot be empty",
                    Status = 400
                });
            }

            if (request.MaxResults < 1 || request.MaxResults > 100)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Invalid max results",
                    Detail = "MaxResults must be between 1 and 100",
                    Status = 400
                });
            }

            var searchTerm = request.Query.Trim();

            _logger.LogInformation("Searching customers with query: {Query}, MaxResults: {MaxResults}", searchTerm, request.MaxResults);

            // Search by phone, email, or address using PostgreSQL case-insensitive ILike
            // ILike will match phone numbers with formatting (e.g., "(555) 123-4567" matches "555")
            var query = _context.Customers
                .Where(c => c.IsActive &&
                    (EF.Functions.ILike(c.Phone, $"%{searchTerm}%") ||
                     EF.Functions.ILike(c.Email, $"%{searchTerm}%") ||
                     EF.Functions.ILike(c.Address, $"%{searchTerm}%")));

            // Order by relevance: exact/starts-with matches first, then contains matches
            var customers = await query
                .OrderBy(c => EF.Functions.ILike(c.Email, $"{searchTerm}%") ? 0 : 1) // Email starts with search term
                .ThenBy(c => EF.Functions.ILike(c.Email, $"%{searchTerm}%") ? 0 : 1) // Email contains search term
                .ThenBy(c => EF.Functions.ILike(c.Phone, $"{searchTerm}%") ? 0 : 1) // Phone starts with search term
                .ThenBy(c => EF.Functions.ILike(c.Phone, $"%{searchTerm}%") ? 0 : 1) // Phone contains search term
                .ThenBy(c => EF.Functions.ILike(c.Address, $"{searchTerm}%") ? 0 : 1) // Address starts with search term
                .ThenBy(c => EF.Functions.ILike(c.Address, $"%{searchTerm}%") ? 0 : 1) // Address contains search term
                .ThenBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Take(request.MaxResults)
                .Select(c => new CustomerSearchResult
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    PostalCode = c.PostalCode,
                    FullAddress = $"{c.Address}, {c.City}, {c.State} {c.PostalCode}"
                })
                .ToListAsync(cancellationToken);

            var response = new CustomerSearchResponse
            {
                Customers = customers,
                TotalCount = customers.Count
            };

            _logger.LogInformation("Found {Count} customers matching query: {Query}", customers.Count, searchTerm);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with query: {Query}", request.Query);
            return Results.Problem(
                title: "An error occurred while searching customers",
                detail: "Please check server logs for more details",
                statusCode: 500
            );
        }
    }
}

