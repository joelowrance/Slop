using MediatR;
using Microsoft.AspNetCore.Mvc;
using VerdaVida.Shared.EndPoints;
using VerdaVida.Shared.MediatrPipelines;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;
using VerdaVidaLawnCare.CoreAPI.Services;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates;

/// <summary>
/// Endpoint for creating estimates
/// </summary>
public class CreateEstimateEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the estimate creation endpoint
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/estimates", async (IMediator m, CreateEstimateRequest request) =>
            {
                return await m.Send(new SubmitEstimateCommand(request));
            })
            .WithName("CreateEstimate")
            .WithSummary("Create a new estimate")
            .WithDescription("Creates a new estimate with customer information and line items. If the customer doesn't exist, they will be created automatically.")
            .WithTags("Estimates")
            .Accepts<CreateEstimateRequest>("application/json")
            .Produces<EstimateResponse>(201)
            .Produces<ValidationProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }
}


public record SubmitEstimateCommand(CreateEstimateRequest Request) : IRequest<IResult>;

public class SubmitEstimateCommandHandler : IRequestHandler<SubmitEstimateCommand, IResult>
{
    private readonly ILogger<SubmitEstimateCommandHandler> _logger;
    private readonly IEstimateService _estimateService;
    private readonly IPythonApiService _pythonApiService;

    public SubmitEstimateCommandHandler(
        ILogger<SubmitEstimateCommandHandler> logger,
        IEstimateService estimateService,
        IPythonApiService pythonApiService)
    {
        _logger = logger;
        _estimateService = estimateService;
        _pythonApiService = pythonApiService;
    }

    public async Task<IResult> Handle(SubmitEstimateCommand command, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var request = command.Request;

        _logger.LogInformation(
            "Starting estimate submission process for customer {CustomerEmail} with {LineItemCount} line items",
            request.Customer.Email,
            request.LineItems?.Count ?? 0);

        // Create the estimate
        _logger.LogInformation("Calling EstimateService to create estimate for customer {CustomerEmail}",
            request.Customer.Email);
        
        var result = await _estimateService.CreateEstimateAsync(request);

        // Call python api
        _logger.LogInformation("Calling Python API to get version information");
        var pyVersionStartTime = DateTime.UtcNow;
        var pyVersion = await _pythonApiService.GetPythonVersionAsync(cancellationToken);
        var pyVersionDuration = (DateTime.UtcNow - pyVersionStartTime).TotalMilliseconds;
        _logger.LogInformation(
            "Python API version call completed in {Duration}ms. Version: {Version}",
            pyVersionDuration,
            pyVersion);

        if (!result.IsSuccess)
        {
            var totalDuration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogWarning(
                "Estimate creation failed after {Duration}ms for customer {CustomerEmail}. Error: {Error}",
                totalDuration,
                request.Customer.Email,
                result.Error);
            return Results.Problem(
                detail: result.Error,
                statusCode: 400,
                title: "Failed to create estimate"
            );
        }

        var totalDurationSuccess = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _logger.LogInformation(
            "Successfully completed estimate submission in {Duration}ms. EstimateNumber: {EstimateNumber}, EstimateId: {EstimateId}, TotalAmount: {TotalAmount}",
            totalDurationSuccess,
            result.Value.EstimateNumber,
            result.Value.Id,
            result.Value.TotalAmount);

        // Return 201 Created with location header
        return Results.CreatedAtRoute(
            "GetEstimate",
            new
            {
                id = result.Value.Id,
                version = pyVersion
            },
            result.Value);
    }
}
