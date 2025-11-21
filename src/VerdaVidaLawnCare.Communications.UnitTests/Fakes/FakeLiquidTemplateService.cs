using VerdaVida.Shared.Common;
using VerdaVidaLawnCare.Communications.Services;

namespace VerdaVidaLawnCare.Communications.UnitTests.Fakes;

/// <summary>
/// Fake implementation of ILiquidTemplateService for unit testing
/// </summary>
public class FakeLiquidTemplateService : ILiquidTemplateService
{
    private readonly Dictionary<string, string> _templates = new();
    public bool ShouldFail { get; set; }
    public string? FailureMessage { get; set; }

    public void AddTemplate(string templateName, string renderedOutput)
    {
        _templates[templateName] = renderedOutput;
    }

    public Task<Result<string>> RenderTemplateAsync<T>(string templateName, T model)
    {
        if (ShouldFail)
        {
            return Task.FromResult(Result<string>.Failure(FailureMessage ?? "Template rendering failed"));
        }

        if (!_templates.TryGetValue(templateName, out var rendered))
        {
            return Task.FromResult(Result<string>.Failure($"Template '{templateName}' not found"));
        }

        return Task.FromResult(Result<string>.Success(rendered));
    }
}




