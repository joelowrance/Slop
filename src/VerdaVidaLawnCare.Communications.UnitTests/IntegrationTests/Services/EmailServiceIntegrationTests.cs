using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VerdaVida.Shared.Common;
using VerdaVidaLawnCare.Communications.Services;
using VerdaVidaLawnCare.Communications.UnitTests.Fixtures;
using Xunit;

namespace VerdaVidaLawnCare.Communications.UnitTests.IntegrationTests.Services;

public class EmailServiceIntegrationTests : BaseIntegrationTest
{
    public EmailServiceIntegrationTests(RabbitMqFixture rabbitMqFixture, MailHogFixture mailHogFixture)
        : base(rabbitMqFixture, mailHogFixture)
    {
    }

    [Fact]
    public async Task SendEmailAsync_WithMailHog_ShouldSendEmailSuccessfully()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var logger = CreateLogger<EmailService>();
        var service = new EmailService(configuration, logger);

        // Act
        var result = await service.SendEmailAsync(
            "recipient@example.com",
            "Test Email Subject",
            "<html><body><h1>Test Email Body</h1></body></html>",
            "sender@example.com",
            "Test Sender");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Note: In a real scenario, you would query MailHog's API to verify the email was received
        // For example: GET http://{MailHogFixture.WebUiHost}:{MailHogFixture.WebUiPort}/api/v2/messages
    }

    private IConfiguration CreateConfiguration()
    {
        var configuration = new Dictionary<string, string?>
        {
            { "Smtp:Host", MailHogFixture.SmtpHost },
            { "Smtp:Port", MailHogFixture.SmtpPort.ToString() },
            { "Smtp:FromEmail", "noreply@test.com" },
            { "Smtp:FromName", "Test Service" },
            { "Smtp:EnableSsl", "false" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .Build();
    }

    private static ILogger<T> CreateLogger<T>()
    {
        return LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<T>();
    }
}

