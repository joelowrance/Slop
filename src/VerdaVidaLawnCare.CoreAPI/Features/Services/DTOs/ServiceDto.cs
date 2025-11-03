namespace VerdaVidaLawnCare.CoreAPI.Features.Services.DTOs;

/// <summary>
/// Data transfer object for service information
/// </summary>
public class ServiceDto
{
    /// <summary>
    /// Gets or sets the service identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the service name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base price for this service
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Gets or sets the type of service
    /// </summary>
    public int ServiceType { get; set; }
}

