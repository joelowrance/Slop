using VerdaVida.Shared.Common;
using VerdaVidaLawnCare.Communications.Models;

namespace VerdaVidaLawnCare.Communications.Services;

/// <summary>
/// Service for fetching weather forecasts
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets a 10-day weather forecast for a given zip code
    /// </summary>
    /// <param name="zipCode">5-digit US zip code</param>
    /// <returns>Result containing the weather forecast or an error message</returns>
    Task<Result<WeatherForecastResponse>> GetForecastAsync(string zipCode);
}

