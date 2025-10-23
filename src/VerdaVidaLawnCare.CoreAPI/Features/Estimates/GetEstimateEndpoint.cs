using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerdaVida.Shared.EndPoints;
using VerdaVidaLawnCare.CoreAPI.Data;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates;

public class GetEstimateEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the estimate creation endpoint
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/estimates/{id:int}", async (IMediator m, int id) => await m.Send(new GetEstimateQuery(id)))
            .WithName("GetEstimate")
            .WithSummary("Gets an existing estimate by ID")
            .WithDescription("Gets and estimate by ID. If the estimate doesn't exist, a 404 Not Found response will be returned.")
            .WithTags("Estimates")
            .Accepts<int>("application/json")
            .Produces<EstimateResponse>(201)
            .Produces<ValidationProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }
}

public record GetEstimateQuery(int Id) : IRequest<EstimateResponse>;

public class GetEstimateQueryHandler : IRequestHandler<GetEstimateQuery, EstimateResponse>
{
    private readonly ApplicationDbContext _context;

    public GetEstimateQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EstimateResponse> Handle(GetEstimateQuery request, CancellationToken cancellationToken)
    {
        var estimate = await _context.Estimates
            .Include(x => x.Customer)
            .Include(x =>x.EstimateLineItems)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (estimate == null)
            throw new KeyNotFoundException($"Estimate with ID {request.Id} was not found.");

        return new EstimateResponse
        {
            Id = estimate.Id,
            EstimateNumber = estimate.EstimateNumber,
            Customer = new CustomerDto
            {
                Id = estimate.Customer.Id,
                FirstName = estimate.Customer.FirstName,
                LastName = estimate.Customer.LastName,
                Email = estimate.Customer.Email,
                Phone = estimate.Customer.Phone,
                FullAddress = estimate.Customer.Address,
                City = estimate.Customer.City,
                State = estimate.Customer.State,
                PostalCode = estimate.Customer.PostalCode
            },
            EstimateDate = estimate.EstimateDate,
            ExpirationDate = estimate.ExpirationDate,
            Status = estimate.Status.ToString(),
            Notes = estimate.Notes,
            Terms = estimate.Terms,
            LineItems = estimate.EstimateLineItems.Select(li => new EstimateLineItemDto
            {
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                LineTotal = li.LineTotal
            }).ToList(),
            Subtotal = estimate.EstimateLineItems.Sum(li => li.LineTotal),
            TaxAmount = 0, // Assuming no tax for now
            TotalAmount = estimate.EstimateLineItems.Sum(li => li.LineTotal),
            CreatedAt = estimate.CreatedAt,
            UpdatedAt = estimate.UpdatedAt,
            DaysUntilExpiration = (int)(estimate.ExpirationDate - DateTimeOffset.UtcNow).TotalDays,
            IsExpired = estimate.ExpirationDate < DateTimeOffset.UtcNow
        };
    }
}

