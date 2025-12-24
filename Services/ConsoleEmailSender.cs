using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace ITHelpDesk.Services;

public class DevConsoleEmailSender : IEmailSender
{
    private readonly ILogger<DevConsoleEmailSender> _logger;

    public DevConsoleEmailSender(ILogger<DevConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("DEV EMAIL → To: {Email} | Subject: {Subject}\n{Body}", email, subject, htmlMessage);
        return Task.CompletedTask;
    }
}
