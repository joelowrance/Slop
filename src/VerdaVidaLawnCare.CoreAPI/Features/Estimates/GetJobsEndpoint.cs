using MediatR;
using Microsoft.AspNetCore.Mvc;
using VerdaVida.Shared.EndPoints;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates;

/// <summary>
/// Endpoint for getting filtered list of jobs
/// </summary>
public class GetJobsEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the get jobs endpoint
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/jobs", 
            async (IMediator m, string? status, string? search, int? customerId) => 
                await m.Send(new GetJobsQuery(status ?? "all", search, customerId)))
            .WithName("GetJobs")
            .WithSummary("Get filtered list of jobs")
            .WithDescription("Retrieves all jobs (estimates excluding cancelled) with optional filters for status and customer search")
            .WithTags("Jobs")
            .Produces<List<EstimateResponse>>(200)
            .Produces<ProblemDetails>(500);
    }
}

public record GetJobsQuery(string Status, string? Search, int? CustomerId) : IRequest<IResult>;

public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, IResult>
{
    private readonly IEstimateService _estimateService;
    private readonly ILogger<GetJobsQueryHandler> _logger;

    public GetJobsQueryHandler(
        IEstimateService estimateService,
        ILogger<GetJobsQueryHandler> logger)
    {
        _estimateService = estimateService;
        _logger = logger;
    }

    public async Task<IResult> Handle(GetJobsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get jobs query - Status: {Status}, Search: {Search}, CustomerId: {CustomerId}", 
            query.Status, query.Search, query.CustomerId);

        var request = new GetJobsRequest
        {
            Status = query.Status,
            Search = query.Search,
            CustomerId = query.CustomerId
        };

        var result = await _estimateService.GetJobsAsync(request);

        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to retrieve jobs: {Error}", result.Error);

            return Results.Problem(new ProblemDetails
            {
                Title = "Failed to retrieve jobs",
                Detail = result.Error,
                Status = 500
            });
        }

        _logger.LogInformation("Successfully retrieved {Count} jobs", result.Value.Count);
        return Results.Ok(result.Value);
    }
}

