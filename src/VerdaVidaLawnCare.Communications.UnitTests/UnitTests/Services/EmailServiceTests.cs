using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VerdaVida.Shared.Common;
using VerdaVidaLawnCare.Communications.Services;
using VerdaVidaLawnCare.Communications.UnitTests.Fixtures;
using Xunit;

namespace VerdaVidaLawnCare.Communications.UnitTests.UnitTests.Services;

public class EmailServiceTests : BaseUnitTest
{
    [Fact]
    public async Task SendEmailAsync_WithEmptyRecipient_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var logger = CreateLogger<EmailService>();
        var service = new EmailService(configuration, logger);

        // Act
        var result = await service.SendEmailAsync(
            string.Empty,
            "Test Subject",
            "<html><body>Test Body</body></html>");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Recipient email address is required");
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptySubject_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var logger = CreateLogger<EmailService>();
        var service = new EmailService(configuration, logger);

        // Act
        var result = await service.SendEmailAsync(
            "test@example.com",
            string.Empty,
            "<html><body>Test Body</body></html>");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Email subject is required");
    }

    [Fact(Skip = "Requires actual SMTP server - use integration tests instead")]
    public Task SendEmailAsync_WithValidParameters_ReturnsSuccess()
    {
        // This test is skipped because it requires an actual SMTP server
        // Use integration tests with MailHog for actual email sending tests
        return Task.CompletedTask;
    }

    private static IConfiguration CreateConfiguration()
    {
        var configuration = new Dictionary<string, string?>
        {
            { "Smtp:Host", "localhost" },
            { "Smtp:Port", "1025" },
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

