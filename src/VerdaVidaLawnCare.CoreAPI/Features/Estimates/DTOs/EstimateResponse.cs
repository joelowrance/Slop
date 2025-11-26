namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

/// <summary>
/// Response model for estimate details
/// </summary>
public class EstimateResponse
{
    /// <summary>
    /// Gets or sets the estimate identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the estimate number
    /// </summary>
    public string EstimateNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer information
    /// </summary>
    public CustomerDto Customer { get; set; } = new();

    /// <summary>
    /// Gets or sets the estimate date
    /// </summary>
    public DateTimeOffset EstimateDate { get; set; }

    /// <summary>
    /// Gets or sets the expiration date
    /// </summary>
    public DateTimeOffset ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the status of the estimate
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets any notes for the estimate
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the terms and conditions
    /// </summary>
    public string Terms { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scheduled date for the job
    /// </summary>
    public DateTimeOffset? ScheduledDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the job was completed
    /// </summary>
    public DateTimeOffset? CompletedDate { get; set; }

    /// <summary>
    /// Gets or sets notes entered when completing the job
    /// </summary>
    public string CompletionNotes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimate line items
    /// </summary>
    public List<EstimateLineItemDto> LineItems { get; set; } = new();

    /// <summary>
    /// Gets or sets the subtotal (sum of all line items)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Gets or sets the tax amount (if applicable)
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the total amount (subtotal + tax)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of days until expiration
    /// </summary>
    public int DaysUntilExpiration { get; set; }

    /// <summary>
    /// Gets or sets whether the estimate is expired
    /// </summary>
    public bool IsExpired { get; set; }
}
