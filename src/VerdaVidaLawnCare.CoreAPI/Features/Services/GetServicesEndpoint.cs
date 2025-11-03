using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerdaVida.Shared.EndPoints;
using VerdaVidaLawnCare.CoreAPI.Data;
using VerdaVidaLawnCare.CoreAPI.Features.Services.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Services;

/// <summary>
/// Endpoint for retrieving active services
/// </summary>
public class GetServicesEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the services retrieval endpoint
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/services", async (IMediator m) => await m.Send(new GetServicesQuery()))
            .WithName("GetServices")
            .WithSummary("Get all active services")
            .WithDescription("Retrieves a list of all active services offered by the company.")
            .WithTags("Services")
            .Produces<List<ServiceDto>>(200)
            .Produces<ProblemDetails>(500);
    }
}

public record GetServicesQuery : IRequest<List<ServiceDto>>;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, List<ServiceDto>>
{
    private readonly ApplicationDbContext _context;

    public GetServicesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceDto>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                BasePrice = s.BasePrice,
                ServiceType = (int)s.ServiceType
            })
            .ToListAsync(cancellationToken);

        return services;
    }
}

