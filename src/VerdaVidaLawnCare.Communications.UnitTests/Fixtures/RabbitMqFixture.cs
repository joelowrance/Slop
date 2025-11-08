using Testcontainers.RabbitMq;
using Xunit;

namespace VerdaVidaLawnCare.Communications.UnitTests.Fixtures;

/// <summary>
/// Fixture for RabbitMQ TestContainer
/// </summary>
public class RabbitMqFixture : IAsyncLifetime
{
    private RabbitMqContainer? _container;

    public string Hostname { get; private set; } = string.Empty;
    public int Port { get; private set; }
    public string Username { get; private set; } = "testuser";
    public string Password { get; private set; } = "testpass";
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _container = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername(Username)
            .WithPassword(Password)
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .Build();

        await _container.StartAsync();

        Hostname = _container.Hostname;
        Port = _container.GetMappedPublicPort(5672);
        ConnectionString = _container.GetConnectionString();
        
        // Wait for RabbitMQ to be fully ready (it takes a few seconds to start)
        await Task.Delay(8000);
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}

