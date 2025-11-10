using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;

namespace VerdaVida.Shared.MediatrPipelines;

/// <summary>
/// Pipeline behavior that logs incoming MediatR requests, results, and exceptions.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestType = typeof(TRequest).FullName ?? typeof(TRequest).Name;

        // Log incoming request
        try
        {
            var requestJson = SerializeSafely(request);
            logger.LogInformation(
                "Handling MediatR request: {RequestName} ({RequestType}) with payload: {RequestPayload}",
                requestName,
                requestType,
                requestJson);
        }
        catch (Exception ex)
        {
            // If serialization fails, log without payload
            logger.LogWarning(
                ex,
                "Failed to serialize request payload for {RequestName} ({RequestType})",
                requestName,
                requestType);
        }

        // Execute handler and catch exceptions
        Exception? exception = null;
        TResponse? response = default;
        var startTime = DateTime.UtcNow;

        try
        {
            response = await next();
            return response;
        }
        catch (ValidationException validationEx)
        {
            // Don't catch ValidationException for IResult responses - let ValidationExceptionBehavior handle it
            // This allows ValidationExceptionBehavior to convert it to ValidationProblemDetails before it bubbles up
            if (typeof(Microsoft.AspNetCore.Http.IResult).IsAssignableFrom(typeof(TResponse)))
            {
                throw;
            }
            
            exception = validationEx;
            logger.LogWarning(
                validationEx,
                "Validation failed for MediatR request: {RequestName} ({RequestType}). Errors: {ValidationErrors}",
                requestName,
                requestType,
                string.Join("; ", validationEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
            throw;
        }
        catch (Exception ex)
        {
            exception = ex;
            logger.LogError(
                ex,
                "Exception occurred while handling MediatR request: {RequestName} ({RequestType})",
                requestName,
                requestType);
            throw;
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;

            // Log response or exception result
            if (exception is not null)
            {
                logger.LogError(
                    exception,
                    "MediatR request failed: {RequestName} ({RequestType}) after {Duration}ms",
                    requestName,
                    requestType,
                    duration.TotalMilliseconds);
            }
            else if (response is not null)
            {
                LogResponse(requestName, requestType, response, duration);
            }
        }
    }

    private void LogResponse(string requestName, string requestType, TResponse response, TimeSpan duration)
    {
        try
        {
            // Check if response is a Result<T> or Result pattern
            var responseType = response.GetType();
            
            // Check for Result<T> pattern
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition().Name.StartsWith("Result`"))
            {
                var isSuccessProperty = responseType.GetProperty("IsSuccess");
                var errorProperty = responseType.GetProperty("Error");
                var valueProperty = responseType.GetProperty("Value");

                if (isSuccessProperty?.GetValue(response) is bool isSuccess)
                {
                    if (isSuccess)
                    {
                        var value = valueProperty?.GetValue(response);
                        var valueJson = value is not null ? SerializeSafely(value) : "null";
                        
                        logger.LogInformation(
                            "MediatR request succeeded: {RequestName} ({RequestType}) in {Duration}ms. Result: {ResultValue}",
                            requestName,
                            requestType,
                            duration.TotalMilliseconds,
                            valueJson);
                    }
                    else
                    {
                        var error = errorProperty?.GetValue(response)?.ToString() ?? "Unknown error";
                        
                        logger.LogWarning(
                            "MediatR request returned failure: {RequestName} ({RequestType}) in {Duration}ms. Error: {Error}",
                            requestName,
                            requestType,
                            duration.TotalMilliseconds,
                            error);
                    }
                }
            }
            // Check for non-generic Result pattern
            else if (responseType.Name == "Result" && !responseType.IsGenericType)
            {
                var isSuccessProperty = responseType.GetProperty("IsSuccess");
                var errorProperty = responseType.GetProperty("Error");

                if (isSuccessProperty?.GetValue(response) is bool isSuccess)
                {
                    if (isSuccess)
                    {
                        logger.LogInformation(
                            "MediatR request succeeded: {RequestName} ({RequestType}) in {Duration}ms",
                            requestName,
                            requestType,
                            duration.TotalMilliseconds);
                    }
                    else
                    {
                        var error = errorProperty?.GetValue(response)?.ToString() ?? "Unknown error";
                        
                        logger.LogWarning(
                            "MediatR request returned failure: {RequestName} ({RequestType}) in {Duration}ms. Error: {Error}",
                            requestName,
                            requestType,
                            duration.TotalMilliseconds,
                            error);
                    }
                }
            }
            else
            {
                // Regular response - log serialized response
                var responseJson = SerializeSafely(response);
                logger.LogInformation(
                    "MediatR request completed: {RequestName} ({RequestType}) in {Duration}ms. Response: {ResponsePayload}",
                    requestName,
                    requestType,
                    duration.TotalMilliseconds,
                    responseJson);
            }
        }
        catch (Exception ex)
        {
            // If response logging fails, log minimal info
            logger.LogWarning(
                ex,
                "Failed to log response for MediatR request: {RequestName} ({RequestType}) in {Duration}ms",
                requestName,
                requestType,
                duration.TotalMilliseconds);
        }
    }

    private static string SerializeSafely(object? obj)
    {
        if (obj is null)
        {
            return "null";
        }

        try
        {
            return JsonSerializer.Serialize(obj, JsonOptions);
        }
        catch (Exception)
        {
            // If serialization fails, return type name
            return $"[{obj.GetType().Name}]";
        }
    }
}

