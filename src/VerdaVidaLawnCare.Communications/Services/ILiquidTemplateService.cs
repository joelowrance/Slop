using VerdaVida.Shared.Common;

namespace VerdaVidaLawnCare.Communications.Services;

/// <summary>
/// Service for rendering Liquid templates
/// </summary>
public interface ILiquidTemplateService
{
    /// <summary>
    /// Renders a Liquid template with the provided model
    /// </summary>
    /// <typeparam name="T">Type of the model</typeparam>
    /// <param name="templateName">Name of the template file (without .liquid extension)</param>
    /// <param name="model">Model object to use for template rendering</param>
    /// <returns>Result containing the rendered HTML string</returns>
    Task<Result<string>> RenderTemplateAsync<T>(string templateName, T model);
}

