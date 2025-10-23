namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

/// <summary>
/// Line item details for estimate requests and responses
/// </summary>
public class EstimateLineItemDto
{
    /// <summary>
    /// Gets or sets the service identifier (optional)
    /// </summary>
    public int? ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the equipment identifier (optional)
    /// </summary>
    public int? EquipmentId { get; set; }

    /// <summary>
    /// Gets or sets the line item description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the line total (quantity * unit price)
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Gets or sets the service name (for response only)
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the equipment name (for response only)
    /// </summary>
    public string? EquipmentName { get; set; }
}
