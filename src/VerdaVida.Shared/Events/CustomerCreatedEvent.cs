namespace VerdaVida.Shared.Events;

/// <summary>
/// Event published when a new customer is created in the system
/// </summary>
public record CustomerCreatedEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the created customer
    /// </summary>
    public int CustomerId { get; init; }

    /// <summary>
    /// Gets or sets the email address of the created customer
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the customer was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}
