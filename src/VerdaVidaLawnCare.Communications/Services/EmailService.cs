using System.Net;
using System.Net.Mail;
using VerdaVida.Shared.Common;

namespace VerdaVidaLawnCare.Communications.Services;

/// <summary>
/// Service for sending emails using SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _smtpSettings = LoadSmtpSettings();
    }

    /// <summary>
    /// Sends an email asynchronously
    /// </summary>
    public async Task<Result<bool>> SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? fromEmail = null,
        string? fromName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                return Result<bool>.Failure("Recipient email address is required");
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                return Result<bool>.Failure("Email subject is required");
            }

            var from = fromEmail ?? _smtpSettings.FromEmail;
            var fromDisplayName = fromName ?? _smtpSettings.FromName;

            _logger.LogInformation(
                "Sending email to {To} with subject {Subject} from {From}",
                to, subject, from);

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(from, fromDisplayName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = string.IsNullOrWhiteSpace(_smtpSettings.Username)
                    ? null
                    : new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
            };

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation(
                "Successfully sent email to {To} with subject {Subject}",
                to, subject);

            return Result<bool>.Success(true);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex,
                "SMTP error sending email to {To} with subject {Subject}: {Error}",
                to, subject, ex.Message);
            return Result<bool>.Failure($"Failed to send email: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error sending email to {To} with subject {Subject}: {Error}",
                to, subject, ex.Message);
            return Result<bool>.Failure($"Failed to send email: {ex.Message}");
        }
    }

    private SmtpSettings LoadSmtpSettings()
    {
        var smtpSection = _configuration.GetSection("Smtp");
        return new SmtpSettings
        {
            Host = smtpSection["Host"] ?? "localhost",
            Port = int.Parse(smtpSection["Port"] ?? "1025"),
            Username = smtpSection["Username"],
            Password = smtpSection["Password"],
            FromEmail = smtpSection["FromEmail"] ?? "noreply@verdevida.com",
            FromName = smtpSection["FromName"] ?? "VerdaVida Lawn Care",
            EnableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "false")
        };
    }

    private class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
    }
}

