namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

/// <summary>
/// Request model for getting jobs list with filters
/// </summary>
public class GetJobsRequest
{
    /// <summary>
    /// Gets or sets the status filter (open, completed, all)
    /// </summary>
    public string Status { get; set; } = "all";

    /// <summary>
    /// Gets or sets the customer search term
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Gets or sets the customer ID filter
    /// </summary>
    public int? CustomerId { get; set; }
}

