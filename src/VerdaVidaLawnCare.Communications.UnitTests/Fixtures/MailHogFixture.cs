using System.Reflection;
using Testcontainers;
using Testcontainers.RabbitMq;
using Xunit;

namespace VerdaVidaLawnCare.Communications.UnitTests.Fixtures;

/// <summary>
/// Fixture for MailHog TestContainer (SMTP testing)
/// Uses reflection to access Testcontainers generic container API
/// </summary>
public class MailHogFixture : IAsyncLifetime
{
    private dynamic? _container;

    public string SmtpHost { get; private set; } = string.Empty;
    public int SmtpPort { get; private set; }
    public string WebUiHost { get; private set; } = string.Empty;
    public int WebUiPort { get; private set; }

    public async Task InitializeAsync()
    {
        // Try to find ContainerBuilder in the base Testcontainers assembly
        // First, get all loaded assemblies that contain "Testcontainers"
        var testcontainersAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null && a.FullName.Contains("Testcontainers"))
            .ToList();
        
        Type? containerBuilderType = null;
        
        foreach (var assembly in testcontainersAssemblies)
        {
            containerBuilderType = assembly.GetTypes()
                .FirstOrDefault(t => 
                    (t.Name == "ContainerBuilder" || t.Name.Contains("ContainerBuilder")) &&
                    t.IsClass && !t.IsAbstract && t.IsPublic);
            
            if (containerBuilderType != null)
                break;
        }
        
        if (containerBuilderType == null)
        {
            throw new InvalidOperationException(
                "ContainerBuilder type not found in any Testcontainers assembly. " +
                "MailHog container support may require a different Testcontainers package.");
        }

        // Create builder instance
        var builder = Activator.CreateInstance(containerBuilderType);
        dynamic dynamicBuilder = builder!;
        
        // Configure container
        dynamicBuilder = dynamicBuilder.WithImage("mailhog/mailhog:latest");
        dynamicBuilder = dynamicBuilder.WithPortBinding(1025, true);
        dynamicBuilder = dynamicBuilder.WithPortBinding(8025, true);
        _container = dynamicBuilder.Build();

        await _container.StartAsync();

        SmtpHost = _container.Hostname;
        SmtpPort = _container.GetMappedPublicPort(1025);
        WebUiHost = _container.Hostname;
        WebUiPort = _container.GetMappedPublicPort(8025);
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}

