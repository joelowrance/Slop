using Microsoft.AspNetCore.Mvc;
using Serilog;
using VerdaVida.Shared.Common;
using VerdaVida.Shared.EndPoints;
using VerdaVida.Shared.Exceptions;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.Validators;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates;

/// <summary>
/// Endpoint for creating estimates
/// </summary>
public class CreateEstimateEndpoint : IEndpoint
{
    private readonly IEstimateService _estimateService;
    private readonly ILogger<CreateEstimateEndpoint> _logger;

    public CreateEstimateEndpoint(IEstimateService estimateService, ILogger<CreateEstimateEndpoint> logger)
    {
        _estimateService = estimateService;
        _logger = logger;
    }

    /// <summary>
    /// Maps the estimate creation endpoint
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/estimates", CreateEstimateAsync)
            .WithName("CreateEstimate")
            .WithSummary("Create a new estimate")
            .WithDescription("Creates a new estimate with customer information and line items. If the customer doesn't exist, they will be created automatically.")
            .WithTags("Estimates")
            .Accepts<CreateEstimateRequest>("application/json")
            .Produces<EstimateResponse>(201)
            .Produces<ValidationProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    /// <summary>
    /// Creates a new estimate
    /// </summary>
    /// <param name="request">The estimate creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created estimate details</returns>
    private async Task<IResult> CreateEstimateAsync(
        [FromBody] CreateEstimateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received request to create estimate for customer {CustomerEmail}",
                request.Customer.Email);

            // Validate the request using FluentValidation
            if (!TryValidateRequest(request, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for estimate creation request: {Errors}",
                    string.Join(", ", validationErrors));
                return Results.ValidationProblem(validationErrors);
            }

            // Create the estimate
            var result = await _estimateService.CreateEstimateAsync(request);

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
                new { id = result.Value.Id },
                result.Value);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in estimate creation: {Error}", ex.Message);
            return Results.Problem(
                detail: ex.Message,
                statusCode: 400,
                title: "Validation error"
            );
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

    /// <summary>
    /// Validates the request using FluentValidation
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="errors">The validation errors if any</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool TryValidateRequest(CreateEstimateRequest request, out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();

        // This is a simplified validation check
        // In a real implementation, you might want to use a validation service
        // or middleware that automatically handles FluentValidation

        var validator = new CreateEstimateRequestValidator();
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            return false;
        }

        return true;
    }
}
