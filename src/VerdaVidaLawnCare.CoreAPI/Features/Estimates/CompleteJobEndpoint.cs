using MediatR;
using Microsoft.AspNetCore.Mvc;
using VerdaVida.Shared.EndPoints;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates;

/// <summary>
/// Endpoint for completing jobs
/// </summary>
public class CompleteJobEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the job completion endpoint
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/jobs/{id:int}/complete", 
            async (IMediator m, int id, CompleteJobRequest request) => 
                await m.Send(new CompleteJobCommand(id, request)))
            .WithName("CompleteJob")
            .WithSummary("Mark a job as completed")
            .WithDescription("Updates an accepted estimate to completed status with completion date and notes")
            .WithTags("Jobs")
            .Accepts<CompleteJobRequest>("application/json")
            .Produces<EstimateResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(500);
    }
}

public record CompleteJobCommand(int EstimateId, CompleteJobRequest Request) : IRequest<IResult>;

public class CompleteJobCommandHandler : IRequestHandler<CompleteJobCommand, IResult>
{
    private readonly IEstimateService _estimateService;
    private readonly ILogger<CompleteJobCommandHandler> _logger;

    public CompleteJobCommandHandler(
        IEstimateService estimateService,
        ILogger<CompleteJobCommandHandler> logger)
    {
        _estimateService = estimateService;
        _logger = logger;
    }

    public async Task<IResult> Handle(CompleteJobCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling complete job command for EstimateId: {EstimateId}", command.EstimateId);

        var result = await _estimateService.CompleteJobAsync(
            command.EstimateId, 
            command.Request.CompletionNotes);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to complete job {EstimateId}: {Error}", 
                command.EstimateId, result.Error);

            return Results.BadRequest(new ProblemDetails
            {
                Title = "Job completion failed",
                Detail = result.Error,
                Status = 400
            });
        }

        _logger.LogInformation("Successfully completed job {EstimateId}", command.EstimateId);
        return Results.Ok(result.Value);
    }
}

