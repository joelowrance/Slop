using VerdaVida.Shared.Events;

namespace VerdaVidaLawnCare.Communications.Models;

/// <summary>
/// Wrapper model that combines EstimateSentEvent and WeatherForecastResponse for email template rendering
/// </summary>
public class EstimateEmailModel
{
    /// <summary>
    /// Gets or sets the estimate event data
    /// </summary>
    public EstimateSentEvent Estimate { get; set; } = null!;

    /// <summary>
    /// Gets or sets the weather forecast data (may be null if weather fetch failed)
    /// </summary>
    public WeatherForecastResponse? Weather { get; set; }

    /// <summary>
    /// Gets the first sunny day from the forecast, or null if no sunny day is found
    /// </summary>
    public DailyForecast? FirstSunnyDay
    {
        get
        {
            if (Weather?.Forecast == null)
            {
                return null;
            }

            return Weather.Forecast.FirstOrDefault(f => f.IsSunny);
        }
    }
}

