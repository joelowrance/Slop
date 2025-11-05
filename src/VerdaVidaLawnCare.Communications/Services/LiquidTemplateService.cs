using Fluid;
using VerdaVida.Shared.Common;
using VerdaVida.Shared.Events;

namespace VerdaVidaLawnCare.Communications.Services;

/// <summary>
/// Service for rendering Liquid templates using Fluid.Core
/// </summary>
public class LiquidTemplateService : ILiquidTemplateService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LiquidTemplateService> _logger;
    private readonly string _templatesPath;
    private readonly FluidParser _parser;
    private readonly Dictionary<string, IFluidTemplate> _templateCache;

    public LiquidTemplateService(IConfiguration configuration, ILogger<LiquidTemplateService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _parser = new FluidParser();
        _templateCache = new Dictionary<string, IFluidTemplate>();

        var templatesPath = _configuration["Templates:Path"] ?? "Templates";
        _templatesPath = Path.IsPathRooted(templatesPath)
            ? templatesPath
            : Path.Combine(AppContext.BaseDirectory, templatesPath);
    }

    /// <summary>
    /// Renders a Liquid template with the provided model
    /// </summary>
    public async Task<Result<string>> RenderTemplateAsync<T>(string templateName, T model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                return Result<string>.Failure("Template name is required");
            }

            var template = await GetTemplateAsync(templateName);
            if (template == null)
            {
                return Result<string>.Failure($"Template '{templateName}' not found");
            }

            if (model == null)
            {
                return Result<string>.Failure("Model cannot be null");
            }

            var templateContext = new TemplateContext(model);
            
            // Configure Fluid to allow member access for all properties
            // This is necessary for accessing nested properties in collections
            // Register the model type and all nested types that might be accessed
            templateContext.Options.MemberAccessStrategy.Register(model.GetType());
            
            // Register member access for common event types that might be nested
            // This allows Fluid to access properties when iterating over collections
            templateContext.Options.MemberAccessStrategy.Register<EstimateLineItemEvent>();
            templateContext.Options.MemberAccessStrategy.Register<EstimateSentEvent>();
            
            // Add common filters and helpers if needed
            // templateContext.Options.Filters.AddFilter("formatDate", FormatDate);

            var rendered = await template.RenderAsync(templateContext);

            _logger.LogDebug(
                "Successfully rendered template {TemplateName}",
                templateName);

            return Result<string>.Success(rendered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error rendering template {TemplateName}: {Error}",
                templateName, ex.Message);
            return Result<string>.Failure($"Failed to render template: {ex.Message}");
        }
    }

    private async Task<IFluidTemplate?> GetTemplateAsync(string templateName)
    {
        // Check cache first
        if (_templateCache.TryGetValue(templateName, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        // Load from file system
        var templatePath = Path.Combine(_templatesPath, $"{templateName}.liquid");
        
        if (!File.Exists(templatePath))
        {
            _logger.LogError(
                "Template file not found: {TemplatePath}",
                templatePath);
            return null;
        }

        try
        {
            var templateContent = await File.ReadAllTextAsync(templatePath);
            
            if (!_parser.TryParse(templateContent, out var template, out var error))
            {
                _logger.LogError(
                    "Failed to parse template {TemplateName}: {Error}",
                    templateName, error);
                return null;
            }

            // Cache the template
            _templateCache[templateName] = template;

            _logger.LogDebug(
                "Loaded and cached template {TemplateName} from {TemplatePath}",
                templateName, templatePath);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error loading template {TemplateName} from {TemplatePath}: {Error}",
                templateName, templatePath, ex.Message);
            return null;
        }
    }
}

