using VerdaVida.Shared.Common;
using VerdaVidaLawnCare.Communications.Services;

namespace VerdaVidaLawnCare.Communications.UnitTests.Fakes;

/// <summary>
/// Fake implementation of IEmailService for unit testing
/// </summary>
public class FakeEmailService : IEmailService
{
    public List<SentEmail> SentEmails { get; } = new();
    public bool ShouldFail { get; set; }
    public string? FailureMessage { get; set; }

    public Task<Result<bool>> SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? fromEmail = null,
        string? fromName = null)
    {
        if (ShouldFail)
        {
            return Task.FromResult(Result<bool>.Failure(FailureMessage ?? "Email sending failed"));
        }

        SentEmails.Add(new SentEmail
        {
            To = to,
            Subject = subject,
            HtmlBody = htmlBody,
            FromEmail = fromEmail,
            FromName = fromName,
            SentAt = DateTime.UtcNow
        });

        return Task.FromResult(Result<bool>.Success(true));
    }

    public class SentEmail
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
        public DateTime SentAt { get; set; }
    }
}


