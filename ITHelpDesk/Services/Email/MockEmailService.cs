using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ITHelpDesk.Services.Email;

/// <summary>
/// Mock email service implementation that writes emails to Console and ILogger.
/// Does not send actual emails - for development and testing purposes only.
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Write to Console
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("📧 EMAIL NOTIFICATION");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body:");
        Console.WriteLine(body);
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        // Log using ILogger
        _logger.LogInformation(
            "Email sent - To: {To}, Subject: {Subject}, Body: {Body}",
            to, subject, body);

        return Task.CompletedTask;
    }
}

