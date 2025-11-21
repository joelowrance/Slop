using System.Diagnostics;
using System.Net.Http.Json;

namespace VerdaVidaLawnCare.CoreAPI.Services;

/// <summary>
/// Service for interacting with the Python API
/// </summary>
public class PythonApiService : IPythonApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonApiService> _logger;
    private static readonly ActivitySource ActivitySource = new("PythonApiService");

    public PythonApiService(HttpClient httpClient, ILogger<PythonApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets the hello message from the Python API
    /// </summary>
    public async Task<string> GetHelloAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetHello");
        activity?.SetTag("endpoint", "/");

        try
        {
            _logger.LogInformation("Calling Python API / endpoint");

            var response = await _httpClient.GetAsync("/", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogInformation("Successfully received response from Python API: {Content}", content);
            activity?.SetTag("response.length", content.Length);

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Python API / endpoint");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request to Python API timed out");
            activity?.SetStatus(ActivityStatusCode.Error, "Request timeout");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Python API / endpoint");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets the Python version information from the Python API
    /// </summary>
    public async Task<string> GetPythonVersionAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetPythonVersion");
        activity?.SetTag("endpoint", "/python");

        try
        {
            _logger.LogInformation("Calling Python API /python endpoint");

            var response = await _httpClient.GetAsync("/python", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogInformation("Successfully received Python version: {Content}", content);
            activity?.SetTag("response.length", content.Length);

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Python API /python endpoint");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request to Python API /python endpoint timed out");
            activity?.SetStatus(ActivityStatusCode.Error, "Request timeout");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Python API /python endpoint");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}




