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
                var result = await m.Send(new SubmitEstimateCommand(request));
                 return result;
                //return await m.Send(new SubmitEstimateCommand(request));
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
        var request = command.Request;

        try
        {
            _logger.LogInformation("Received request to create estimate for customer {CustomerEmail}",
                request.Customer.Email);

            // Create the estimate
            var result = await _estimateService.CreateEstimateAsync(request);

            // Call python api
            var pyVersion = await _pythonApiService.GetPythonVersionAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to create estimate: {Error}", result.Error);
                return Results.Problem(
                    detail: result.Error,
                    statusCode: 400,
                    title: "Failed to create estimate"
                );
            }

            _logger.LogInformation("Successfully created estimate {EstimateNumber} with ID {EstimateId}",
                result.Value.EstimateNumber, result.Value.Id);

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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for estimate creation: {Errors}",
                string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            // Convert ValidationException to ValidationProblemDetails format
            // Strip "Request." prefix from property names since the API accepts CreateEstimateRequest directly
            var validationErrors = ex.Errors
                .GroupBy(e => e.PropertyName.StartsWith("Request.", StringComparison.Ordinal)
                    ? e.PropertyName["Request.".Length..]
                    : e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return Results.ValidationProblem(validationErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating estimate for customer {CustomerEmail}",
                request.Customer.Email);
            return Results.Problem(
                detail: "An unexpected error occurred while creating the estimate",
                statusCode: 500,
                title: "Internal server error"
            );
        }
    }
}
