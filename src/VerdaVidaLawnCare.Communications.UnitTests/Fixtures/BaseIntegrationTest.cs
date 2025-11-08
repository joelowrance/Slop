using Xunit;

namespace VerdaVidaLawnCare.Communications.UnitTests.Fixtures;

/// <summary>
/// Base class for integration tests with TestContainers setup
/// </summary>
public abstract class BaseIntegrationTest : IClassFixture<RabbitMqFixture>, IClassFixture<MailHogFixture>
{
    protected readonly RabbitMqFixture RabbitMqFixture;
    protected readonly MailHogFixture MailHogFixture;

    protected BaseIntegrationTest(RabbitMqFixture rabbitMqFixture, MailHogFixture mailHogFixture)
    {
        RabbitMqFixture = rabbitMqFixture;
        MailHogFixture = mailHogFixture;
    }
}

