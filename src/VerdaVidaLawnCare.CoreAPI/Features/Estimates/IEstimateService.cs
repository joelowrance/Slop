using VerdaVida.Shared.Common;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates;

/// <summary>
/// Service interface for estimate operations
/// </summary>
public interface IEstimateService
{
    /// <summary>
    /// Creates a new estimate with the provided information
    /// </summary>
    /// <param name="request">The estimate creation request</param>
    /// <returns>A result containing the created estimate details or an error message</returns>
    Task<Result<EstimateResponse>> CreateEstimateAsync(CreateEstimateRequest request);

    /// <summary>
    /// Sends an estimate to the customer by updating its status to Sent and publishing an event
    /// </summary>
    /// <param name="estimateId">The estimate identifier</param>
    /// <returns>A result containing the updated estimate details or an error message</returns>
    Task<Result<EstimateResponse>> SendEstimateAsync(int estimateId);
}
