using VerdaVida.Shared.Common;

namespace VerdaVidaLawnCare.Communications.Services;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML email body</param>
    /// <param name="fromEmail">Optional sender email address</param>
    /// <param name="fromName">Optional sender name</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? fromEmail = null,
        string? fromName = null);
}

