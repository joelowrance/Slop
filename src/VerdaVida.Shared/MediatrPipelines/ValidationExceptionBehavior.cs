using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VerdaVida.Shared.MediatrPipelines;

/// <summary>
/// Pipeline behavior that catches ValidationException and converts it to ValidationProblemDetails IResult
/// for requests that return IResult.
/// </summary>
public class ValidationExceptionBehavior<TRequest, TResponse>(
    ILogger<ValidationExceptionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (ValidationException ex)
        {
            logger.LogInformation(
                "ValidationExceptionBehavior caught ValidationException for {RequestName}. TResponse type: {ResponseType}, IsIResult: {IsIResult}",
                typeof(TRequest).Name,
                typeof(TResponse).FullName,
                typeof(IResult).IsAssignableFrom(typeof(TResponse)));

            // Only handle ValidationException for IResult responses
            if (typeof(IResult).IsAssignableFrom(typeof(TResponse)))
            {
                logger.LogWarning(
                    ex,
                    "Validation failed for MediatR request: {RequestName}. Errors: {ValidationErrors}",
                    typeof(TRequest).Name,
                    string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

                // Convert ValidationException to ValidationProblemDetails format
                // Strip "Request." prefix from property names since the API accepts the request DTO directly
                var validationErrors = ex.Errors
                    .GroupBy(e => e.PropertyName.StartsWith("Request.", StringComparison.Ordinal)
                        ? e.PropertyName["Request.".Length..]
                        : e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                var result = Results.ValidationProblem(validationErrors);
                logger.LogInformation("ValidationExceptionBehavior converting to ValidationProblemDetails with {ErrorCount} error groups", validationErrors.Count);
                return (TResponse)(object)result;
            }

            logger.LogInformation("ValidationExceptionBehavior re-throwing ValidationException for non-IResult response type");
            // For non-IResult responses, re-throw the exception
            throw;
        }
    }
}

