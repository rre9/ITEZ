using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ITHelpDesk.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly DevConsoleEmailSender _fallback;

    public SmtpEmailSender(
        IOptions<EmailSettings> options,
        ILogger<SmtpEmailSender> logger,
        DevConsoleEmailSender fallback)
    {
        _settings = options.Value;
        _logger = logger;
        _fallback = fallback;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogWarning("EmailSettings not configured. Falling back to development console sender.");
            await _fallback.SendEmailAsync(email, subject, htmlMessage);
            return;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.From!),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            message.To.Add(email);

            using var client = new SmtpClient(_settings.Host!, _settings.Port ?? 587)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_settings.UserName) &&
                !string.IsNullOrWhiteSpace(_settings.Password))
            {
                client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
            }

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP to {Email}. Falling back to console sender.", email);
            await _fallback.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}

