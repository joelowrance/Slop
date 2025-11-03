using MediatR;
using Microsoft.AspNetCore.Mvc;
using VerdaVida.Shared.Common;
using VerdaVida.Shared.EndPoints;
using VerdaVidaLawnCare.CoreAPI.Data;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.Validators;
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
    private readonly ApplicationDbContext _context;

    public SubmitEstimateCommandHandler(
        ILogger<SubmitEstimateCommandHandler> logger,
        IEstimateService estimateService,
        IPythonApiService pythonApiService,
        ApplicationDbContext context)
    {
        _logger = logger;
        _estimateService = estimateService;
        _pythonApiService = pythonApiService;
        _context = context;
    }

    public async Task<IResult> Handle(SubmitEstimateCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        try
        {
            _logger.LogInformation("Received request to create estimate for customer {CustomerEmail}",
                request.Customer.Email);

            // Validate the request using FluentValidation
            var (isValid, validationErrors) = await TryValidateRequestAsync(request, _context, cancellationToken);
            if (!isValid)
            {
                _logger.LogWarning("Validation failed for estimate creation request: {Errors}",
                    string.Join(", ", validationErrors.Values.SelectMany(v => v)));
                return Results.ValidationProblem(validationErrors);
            }

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
    /// <param name="context">The database context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A tuple indicating if valid and any validation errors</returns>
    private static async Task<(bool IsValid, Dictionary<string, string[]> Errors)> TryValidateRequestAsync(
        CreateEstimateRequest request, 
        ApplicationDbContext context, 
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        // This is a simplified validation check
        // In a real implementation, you might want to use a validation service
        // or middleware that automatically handles FluentValidation

        var validator = new CreateEstimateRequestValidator(context);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            return (false, errors);
        }

        return (true, errors);
    }
}
