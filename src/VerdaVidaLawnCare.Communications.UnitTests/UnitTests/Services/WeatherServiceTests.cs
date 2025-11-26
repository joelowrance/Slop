using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using VerdaVidaLawnCare.Communications.Models;
using VerdaVidaLawnCare.Communications.Services;
using VerdaVidaLawnCare.Communications.UnitTests.Fixtures;
using Xunit;

namespace VerdaVidaLawnCare.Communications.UnitTests.UnitTests.Services;

public class WeatherServiceTests : BaseUnitTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<ILogger<WeatherService>> _loggerMock;
    private readonly HttpClient _httpClient;
    private readonly WeatherService _service;

    public WeatherServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://test-weather-api.com")
        };
        _loggerMock = new Mock<ILogger<WeatherService>>();
        _service = new WeatherService(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetForecastAsync_WithValidZipCode_ReturnsSuccessAndForecast()
    {
        // Arrange
        var zipCode = "12345";
        var expectedForecast = new WeatherForecastResponse
        {
            Location = "Test City",
            Forecast = new List<DailyForecast>
            {
                new DailyForecast { Date = DateTime.Today.ToString("yyyy-MM-dd"), TemperatureHigh = 75, Description = "Sunny" }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedForecast);
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().EndsWith($"/forecast/{zipCode}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _service.GetForecastAsync(zipCode);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Location.Should().Be("Test City");
        result.Value.Forecast.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetForecastAsync_WithInvalidZipCode_ReturnsFailure()
    {
        // Arrange
        var invalidZipCode = "123";

        // Act
        var result = await _service.GetForecastAsync(invalidZipCode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid zip code format");
        
        // Verify HttpClient was NOT called
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }
    
    [Fact]
    public async Task GetForecastAsync_WithEmptyZipCode_ReturnsFailure()
    {
        // Arrange
        var emptyZipCode = "";

        // Act
        var result = await _service.GetForecastAsync(emptyZipCode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Zip code is required");
    }

    [Fact]
    public async Task GetForecastAsync_WhenApiReturnsNotFound_ReturnsFailure()
    {
        // Arrange
        var zipCode = "99999";
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Not Found")
            });

        // Act
        var result = await _service.GetForecastAsync(zipCode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Weather data not found");
    }

    [Fact]
    public async Task GetForecastAsync_WhenApiReturnsServerError_ReturnsFailure()
    {
        // Arrange
        var zipCode = "12345";
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal Server Error")
            });

        // Act
        var result = await _service.GetForecastAsync(zipCode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Weather service error");
    }

    [Fact]
    public async Task GetForecastAsync_WhenRequestTimesOut_ReturnsFailure()
    {
        // Arrange
        var zipCode = "12345";
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException(null, new TimeoutException()));

        // Act
        var result = await _service.GetForecastAsync(zipCode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("timed out");
    }

    [Fact]
    public async Task GetForecastAsync_WhenResponseIsMalformed_ReturnsFailure()
    {
        // Arrange
        var zipCode = "12345";
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ invalid json }")
            });

        // Act
        var result = await _service.GetForecastAsync(zipCode);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to parse weather forecast response");
    }
}

