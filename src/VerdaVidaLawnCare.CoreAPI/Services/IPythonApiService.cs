namespace VerdaVidaLawnCare.CoreAPI.Services;

/// <summary>
/// Service interface for interacting with the Python API
/// </summary>
public interface IPythonApiService
{
    /// <summary>
    /// Gets the hello message from the Python API
    /// </summary>
    /// <returns>A result containing the hello message or an error</returns>
    Task<string> GetHelloAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Python version information from the Python API
    /// </summary>
    /// <returns>A result containing the Python version or an error</returns>
    Task<string> GetPythonVersionAsync(CancellationToken cancellationToken = default);
}





