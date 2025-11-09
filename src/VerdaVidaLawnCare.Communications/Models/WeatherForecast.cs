using System.Text.Json.Serialization;

namespace VerdaVidaLawnCare.Communications.Models;

/// <summary>
/// Represents a weather forecast response from the weather API
/// </summary>
public class WeatherForecastResponse
{
    /// <summary>
    /// Gets or sets the zip code for the forecast
    /// </summary>
    [JsonPropertyName("zip_code")]
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location name
    /// </summary>
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country code
    /// </summary>
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the daily forecast list
    /// </summary>
    [JsonPropertyName("forecast")]
    public List<DailyForecast> Forecast { get; set; } = new();
}

/// <summary>
/// Represents a daily weather forecast
/// </summary>
public class DailyForecast
{
    /// <summary>
    /// Gets or sets the date in YYYY-MM-DD format
    /// </summary>
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the high temperature in Fahrenheit
    /// </summary>
    [JsonPropertyName("temperature_high")]
    public double TemperatureHigh { get; set; }

    /// <summary>
    /// Gets or sets the low temperature in Fahrenheit
    /// </summary>
    [JsonPropertyName("temperature_low")]
    public double TemperatureLow { get; set; }

    /// <summary>
    /// Gets or sets the average temperature in Fahrenheit
    /// </summary>
    [JsonPropertyName("temperature_avg")]
    public double TemperatureAvg { get; set; }

    /// <summary>
    /// Gets or sets the weather description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the main weather condition (e.g., "Clear", "Clouds", "Rain")
    /// </summary>
    [JsonPropertyName("condition")]
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the average humidity percentage
    /// </summary>
    [JsonPropertyName("humidity")]
    public double Humidity { get; set; }

    /// <summary>
    /// Gets or sets the average wind speed
    /// </summary>
    [JsonPropertyName("wind_speed")]
    public double WindSpeed { get; set; }

    /// <summary>
    /// Gets a value indicating whether this day is considered "sunny" (Clear condition)
    /// </summary>
    public bool IsSunny => string.Equals(Condition, "Clear", StringComparison.OrdinalIgnoreCase);
}

