using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VerdaVida.Shared.Common;
using VerdaVida.Shared.Events;
using VerdaVidaLawnCare.Communications.Services;
using VerdaVidaLawnCare.Communications.UnitTests.Fixtures;
using Xunit;

namespace VerdaVidaLawnCare.Communications.UnitTests.UnitTests.Services;

public class LiquidTemplateServiceTests : BaseUnitTest
{
    [Fact]
    public async Task RenderTemplateAsync_WithValidTemplate_ReturnsRenderedContent()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var logger = CreateLogger<LiquidTemplateService>();
        var service = new LiquidTemplateService(configuration, logger);

        // Create a test template file
        var templatePath = Path.Combine("Templates", "TestTemplate.liquid");
        var templateDir = Path.GetDirectoryName(templatePath);
        if (!string.IsNullOrEmpty(templateDir) && !Directory.Exists(templateDir))
        {
            Directory.CreateDirectory(templateDir);
        }

        var templateContent = "Hello {{ Name }}!";
        await File.WriteAllTextAsync(templatePath, templateContent);

        try
        {
            var model = new { Name = "World" };

            // Act
            var result = await service.RenderTemplateAsync("TestTemplate", model);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Contain("Hello World!");
        }
        finally
        {
            // Cleanup
            if (File.Exists(templatePath))
            {
                File.Delete(templatePath);
            }
        }
    }

    [Fact]
    public async Task RenderTemplateAsync_WithNonExistentTemplate_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var logger = CreateLogger<LiquidTemplateService>();
        var service = new LiquidTemplateService(configuration, logger);

        // Act
        var result = await service.RenderTemplateAsync("NonExistentTemplate", new { });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task RenderTemplateAsync_WithEmptyTemplateName_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var logger = CreateLogger<LiquidTemplateService>();
        var service = new LiquidTemplateService(configuration, logger);

        // Act
        var result = await service.RenderTemplateAsync(string.Empty, new { });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Template name is required");
    }

    [Fact]
    public async Task RenderTemplateAsync_WithNullModel_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var logger = CreateLogger<LiquidTemplateService>();
        var service = new LiquidTemplateService(configuration, logger);

        // Create a test template file
        var templatePath = Path.Combine("Templates", "TestTemplate.liquid");
        var templateDir = Path.GetDirectoryName(templatePath);
        if (!string.IsNullOrEmpty(templateDir) && !Directory.Exists(templateDir))
        {
            Directory.CreateDirectory(templateDir);
        }

        await File.WriteAllTextAsync(templatePath, "Test");

        try
        {
            // Act
            var result = await service.RenderTemplateAsync<object?>("TestTemplate", null);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Model cannot be null");
        }
        finally
        {
            // Cleanup
            if (File.Exists(templatePath))
            {
                File.Delete(templatePath);
            }
        }
    }

    private static IConfiguration CreateConfiguration()
    {
        var configuration = new Dictionary<string, string?>
        {
            { "Templates:Path", "Templates" }
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

