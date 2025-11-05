namespace VerdaVida.Shared.Events;

/// <summary>
/// Event published when an estimate is sent to a customer
/// </summary>
public record EstimateSentEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the estimate
    /// </summary>
    public int EstimateId { get; init; }

    /// <summary>
    /// Gets or sets the estimate number
    /// </summary>
    public string EstimateNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; init; }

    /// <summary>
    /// Gets or sets the customer's first name
    /// </summary>
    public string CustomerFirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's last name
    /// </summary>
    public string CustomerLastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address
    /// </summary>
    public string CustomerEmail { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimate date
    /// </summary>
    public DateTimeOffset EstimateDate { get; init; }

    /// <summary>
    /// Gets or sets the expiration date
    /// </summary>
    public DateTimeOffset ExpirationDate { get; init; }

    /// <summary>
    /// Gets or sets the notes for the estimate
    /// </summary>
    public string Notes { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the terms and conditions
    /// </summary>
    public string Terms { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimate line items
    /// </summary>
    public List<EstimateLineItemEvent> LineItems { get; init; } = new();

    /// <summary>
    /// Gets or sets the subtotal amount
    /// </summary>
    public decimal Subtotal { get; init; }

    /// <summary>
    /// Gets or sets the tax amount
    /// </summary>
    public decimal TaxAmount { get; init; }

    /// <summary>
    /// Gets or sets the total amount
    /// </summary>
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// Gets or sets the timestamp when the estimate was sent
    /// </summary>
    public DateTimeOffset SentAt { get; init; }
}

/// <summary>
/// Represents a line item in an estimate event
/// </summary>
public record EstimateLineItemEvent
{
    /// <summary>
    /// Gets or sets the line item description
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Gets or sets the unit price
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// Gets or sets the line total
    /// </summary>
    public decimal LineTotal { get; init; }
}

