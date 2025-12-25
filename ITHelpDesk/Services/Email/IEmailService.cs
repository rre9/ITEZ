using System.Threading.Tasks;

namespace ITHelpDesk.Services.Email;

/// <summary>
/// Service interface for sending emails in the Access Request workflow.
/// This is a mock implementation that logs emails to console and logger.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML or plain text)</param>
    Task SendEmailAsync(string to, string subject, string body);
}

