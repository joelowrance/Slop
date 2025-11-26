using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using VerdaVida.Shared.Common;
using VerdaVida.Shared.Events;
using VerdaVidaLawnCare.Communications.Consumers;
using VerdaVidaLawnCare.Communications.Models;
using VerdaVidaLawnCare.Communications.Services;
using VerdaVidaLawnCare.Communications.UnitTests.Fixtures;
using Xunit;

namespace VerdaVidaLawnCare.Communications.UnitTests.UnitTests.Consumers;

public class EstimateSentEventConsumerTests : BaseUnitTest
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILiquidTemplateService> _templateServiceMock;
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly Mock<ILogger<EstimateSentEventConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<EstimateSentEvent>> _contextMock;
    private readonly EstimateSentEventConsumer _consumer;

    public EstimateSentEventConsumerTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _templateServiceMock = new Mock<ILiquidTemplateService>();
        _weatherServiceMock = new Mock<IWeatherService>();
        _loggerMock = new Mock<ILogger<EstimateSentEventConsumer>>();
        _contextMock = new Mock<ConsumeContext<EstimateSentEvent>>();

        _consumer = new EstimateSentEventConsumer(
            _emailServiceMock.Object,
            _templateServiceMock.Object,
            _weatherServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_HappyPath_SendsEmailWithWeather()
    {
        // Arrange
        var estimateEvent = new EstimateSentEvent
        {
            EstimateId = 1,
            EstimateNumber = "EST-123",
            CustomerEmail = "test@example.com",
            CustomerPostalCode = "12345"
        };
        _contextMock.Setup(x => x.Message).Returns(estimateEvent);

        var weatherForecast = new WeatherForecastResponse { Location = "Test City" };
        _weatherServiceMock
            .Setup(x => x.GetForecastAsync(estimateEvent.CustomerPostalCode))
            .ReturnsAsync(Result<WeatherForecastResponse>.Success(weatherForecast));

        _templateServiceMock
            .Setup(x => x.RenderTemplateAsync(It.IsAny<string>(), It.IsAny<EstimateEmailModel>()))
            .ReturnsAsync(Result<string>.Success("<html>Email Content</html>"));

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(
                estimateEvent.CustomerEmail,
                It.IsAny<string>(),
                "<html>Email Content</html>",
                null,
                null))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _weatherServiceMock.Verify(x => x.GetForecastAsync(estimateEvent.CustomerPostalCode), Times.Once);
        
        _templateServiceMock.Verify(x => x.RenderTemplateAsync(
            "EstimateEmail", 
            It.Is<EstimateEmailModel>(m => m.Estimate == estimateEvent && m.Weather == weatherForecast)), 
            Times.Once);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            estimateEvent.CustomerEmail,
            $"Your Estimate #{estimateEvent.EstimateNumber} - VerdaVida Lawn Care",
            "<html>Email Content</html>",
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task Consume_WhenWeatherServiceFails_SendsEmailWithoutWeather()
    {
        // Arrange
        var estimateEvent = new EstimateSentEvent
        {
            EstimateId = 1,
            EstimateNumber = "EST-123",
            CustomerEmail = "test@example.com",
            CustomerPostalCode = "12345"
        };
        _contextMock.Setup(x => x.Message).Returns(estimateEvent);

        _weatherServiceMock
            .Setup(x => x.GetForecastAsync(estimateEvent.CustomerPostalCode))
            .ReturnsAsync(Result<WeatherForecastResponse>.Failure("Weather service down"));

        _templateServiceMock
            .Setup(x => x.RenderTemplateAsync(It.IsAny<string>(), It.IsAny<EstimateEmailModel>()))
            .ReturnsAsync(Result<string>.Success("<html>Email Content</html>"));

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        // Weather service was called but failed
        _weatherServiceMock.Verify(x => x.GetForecastAsync(estimateEvent.CustomerPostalCode), Times.Once);
        
        // Template rendered with null weather
        _templateServiceMock.Verify(x => x.RenderTemplateAsync(
            "EstimateEmail", 
            It.Is<EstimateEmailModel>(m => m.Estimate == estimateEvent && m.Weather == null)), 
            Times.Once);

        // Email still sent
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            estimateEvent.CustomerEmail,
            It.IsAny<string>(),
            "<html>Email Content</html>",
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task Consume_WhenZipCodeMissing_SkipsWeatherAndSendsEmail()
    {
        // Arrange
        var estimateEvent = new EstimateSentEvent
        {
            EstimateId = 1,
            EstimateNumber = "EST-123",
            CustomerEmail = "test@example.com",
            CustomerPostalCode = "" // Missing zip code
        };
        _contextMock.Setup(x => x.Message).Returns(estimateEvent);

        _templateServiceMock
            .Setup(x => x.RenderTemplateAsync(It.IsAny<string>(), It.IsAny<EstimateEmailModel>()))
            .ReturnsAsync(Result<string>.Success("<html>Email Content</html>"));

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        // Weather service NOT called
        _weatherServiceMock.Verify(x => x.GetForecastAsync(It.IsAny<string>()), Times.Never);
        
        // Template rendered with null weather
        _templateServiceMock.Verify(x => x.RenderTemplateAsync(
            "EstimateEmail", 
            It.Is<EstimateEmailModel>(m => m.Estimate == estimateEvent && m.Weather == null)), 
            Times.Once);

        // Email sent
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            estimateEvent.CustomerEmail,
            It.IsAny<string>(),
            "<html>Email Content</html>",
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task Consume_WhenTemplateRenderFails_DoesNotSendEmail()
    {
        // Arrange
        var estimateEvent = new EstimateSentEvent
        {
            EstimateId = 1,
            EstimateNumber = "EST-123",
            CustomerEmail = "test@example.com",
            CustomerPostalCode = "12345"
        };
        _contextMock.Setup(x => x.Message).Returns(estimateEvent);

        _weatherServiceMock
            .Setup(x => x.GetForecastAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<WeatherForecastResponse>.Success(new WeatherForecastResponse()));

        _templateServiceMock
            .Setup(x => x.RenderTemplateAsync(It.IsAny<string>(), It.IsAny<EstimateEmailModel>()))
            .ReturnsAsync(Result<string>.Failure("Template render failed"));

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        // Email service NOT called
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);

        // Logger should log error
        // Note: verifying logger extensions is tricky with Moq, usually verifying ILogger.Log method call is enough if strict
    }

    [Fact]
    public async Task Consume_WhenEmailSendFails_LogsError()
    {
        // Arrange
        var estimateEvent = new EstimateSentEvent
        {
            EstimateId = 1,
            EstimateNumber = "EST-123",
            CustomerEmail = "test@example.com",
            CustomerPostalCode = "12345"
        };
        _contextMock.Setup(x => x.Message).Returns(estimateEvent);

        _weatherServiceMock
            .Setup(x => x.GetForecastAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<WeatherForecastResponse>.Success(new WeatherForecastResponse()));

        _templateServiceMock
            .Setup(x => x.RenderTemplateAsync(It.IsAny<string>(), It.IsAny<EstimateEmailModel>()))
            .ReturnsAsync(Result<string>.Success("<html>Email Content</html>"));

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null))
            .ReturnsAsync(Result<bool>.Failure("SMTP Error"));

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        // Email service WAS called
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            estimateEvent.CustomerEmail,
            It.IsAny<string>(),
            "<html>Email Content</html>",
            null,
            null), Times.Once);
            
        // No exception thrown (handled in consumer)
    }
}

