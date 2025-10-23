using MediatR;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

/// <summary>
/// Request model for creating a new estimate
/// </summary>
public class CreateEstimateRequest
{
    /// <summary>
    /// Gets or sets the customer information
    /// </summary>
    public CustomerInfoDto Customer { get; set; } = new();

    /// <summary>
    /// Gets or sets the estimate line items
    /// </summary>
    public List<EstimateLineItemDto> LineItems { get; set; } = new();

    /// <summary>
    /// Gets or sets any notes for the estimate
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the terms and conditions
    /// </summary>
    public string Terms { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration date (optional, defaults to 30 days from creation)
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
}

/// <summary>
/// Customer information for creating estimates
/// </summary>
public class CustomerInfoDto
{
    /// <summary>
    /// Gets or sets the customer's first name
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
    /// Gets or sets the customer's street address
    /// </summary>
    public string Address { get; set; } = string.Empty;

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

