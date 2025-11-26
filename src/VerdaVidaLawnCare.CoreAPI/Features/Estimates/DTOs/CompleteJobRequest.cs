namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

/// <summary>
/// Request model for completing a job
/// </summary>
public class CompleteJobRequest
{
    /// <summary>
    /// Gets or sets notes about the job completion
    /// </summary>
    public string CompletionNotes { get; set; } = string.Empty;
}

