namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

/// <summary>
/// Customer information for estimate responses
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the customer's full name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's full address
    /// </summary>
    public string FullAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state or province
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
}
