namespace VerdaVidaLawnCare.CoreAPI.Features.Customers.DTOs;

/// <summary>
/// Request model for searching customers
/// </summary>
public class CustomerSearchRequest
{
    /// <summary>
    /// Gets or sets the search query string
    /// Searches across phone, email, and street address
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of results to return (default: 20)
    /// </summary>
    public int MaxResults { get; set; } = 20;
}

/// <summary>
/// Customer search result model
/// </summary>
public class CustomerSearchResult
{
    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int Id { get; set; }

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

    /// <summary>
    /// Gets or sets the full formatted address for display
    /// </summary>
    public string FullAddress { get; set; } = string.Empty;
}

/// <summary>
/// Response model for customer search
/// </summary>
public class CustomerSearchResponse
{
    /// <summary>
    /// Gets or sets the list of matching customers
    /// </summary>
    public List<CustomerSearchResult> Customers { get; set; } = new();

    /// <summary>
    /// Gets or sets the total number of matches found
    /// </summary>
    public int TotalCount { get; set; }
}

