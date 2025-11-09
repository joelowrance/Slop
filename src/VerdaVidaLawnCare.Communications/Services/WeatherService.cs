using System.Net.Http.Json;
using System.Text.Json;
using VerdaVida.Shared.Common;
using VerdaVidaLawnCare.Communications.Models;

namespace VerdaVidaLawnCare.Communications.Services;

/// <summary>
/// Service for fetching weather forecasts from the VerdeVida.Weather API
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Gets a 10-day weather forecast for a given zip code
    /// </summary>
    public async Task<Result<WeatherForecastResponse>> GetForecastAsync(string zipCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                return Result<WeatherForecastResponse>.Failure("Zip code is required");
            }

            // Validate zip code format (5 digits)
            if (!System.Text.RegularExpressions.Regex.IsMatch(zipCode, @"^\d{5}$"))
            {
                _logger.LogWarning("Invalid zip code format: {ZipCode}", zipCode);
                return Result<WeatherForecastResponse>.Failure("Invalid zip code format. Please provide a 5-digit US zip code.");
            }

            _logger.LogInformation("Fetching weather forecast for zip code: {ZipCode}", zipCode);

            var response = await _httpClient.GetAsync($"/forecast/{zipCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Weather API returned error status {StatusCode} for zip code {ZipCode}: {Error}",
                    (int)response.StatusCode,
                    zipCode,
                    errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Result<WeatherForecastResponse>.Failure($"Weather data not found for zip code: {zipCode}");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout ||
                    response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                {
                    return Result<WeatherForecastResponse>.Failure("Weather service is currently unavailable. Please try again later.");
                }

                return Result<WeatherForecastResponse>.Failure($"Weather service error: {response.StatusCode}");
            }

            var forecast = await response.Content.ReadFromJsonAsync<WeatherForecastResponse>(_jsonOptions);

            if (forecast == null)
            {
                _logger.LogError("Failed to deserialize weather forecast response for zip code: {ZipCode}", zipCode);
                return Result<WeatherForecastResponse>.Failure("Failed to parse weather forecast response");
            }

            _logger.LogInformation(
                "Successfully retrieved weather forecast for zip code: {ZipCode}, Location: {Location}, Days: {DayCount}",
                zipCode,
                forecast.Location,
                forecast.Forecast.Count);

            return Result<WeatherForecastResponse>.Success(forecast);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching weather forecast for zip code: {ZipCode}", zipCode);
            return Result<WeatherForecastResponse>.Failure("Unable to connect to weather service. Please try again later.");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning(ex, "Timeout fetching weather forecast for zip code: {ZipCode}", zipCode);
            return Result<WeatherForecastResponse>.Failure("Weather service request timed out. Please try again later.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for weather forecast response for zip code: {ZipCode}", zipCode);
            return Result<WeatherForecastResponse>.Failure("Failed to parse weather forecast response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching weather forecast for zip code: {ZipCode}", zipCode);
            return Result<WeatherForecastResponse>.Failure($"Unexpected error: {ex.Message}");
        }
    }
}

