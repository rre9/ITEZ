using System;
using System.Threading.Tasks;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.Services.Notifications;

/// <summary>
/// Default implementation of INotificationService using SMTP email.
/// 
/// FUTURE INTEGRATION NOTE:
/// This implementation can be replaced with a Microsoft Graph-based service
/// (e.g., GraphNotificationService) without requiring changes to any controller code.
/// Simply register the new implementation in Program.cs and replace this registration.
/// 
/// All email templates and notification logic are contained within this service
/// to ensure clean separation of concerns.
/// </summary>
public class EmailNotificationService : INotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmailNotificationService(
        IEmailSender emailSender,
        ILogger<EmailNotificationService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _emailSender = emailSender;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task NotifyManagerAsync(AccessRequest accessRequest)
    {
        if (accessRequest?.Ticket == null || accessRequest.SelectedManager == null)
        {
            _logger.LogWarning("Cannot send manager notification: AccessRequest or related entities are null");
            return;
        }

        try
        {
            var ticket = accessRequest.Ticket;
            var ticketNumber = $"HD-{ticket.Id:D6}";
            var manager = accessRequest.SelectedManager;
            var managerEmail = manager.Email ?? string.Empty;

            if (string.IsNullOrWhiteSpace(managerEmail))
            {
                _logger.LogWarning("Cannot send notification to manager {ManagerId}: email address is missing", manager.Id);
                return;
            }

            var approvalUrl = GenerateApprovalUrl(ticket.Id, "ApproveAccessRequest");
            var subject = $"[IT Help Desk] Access Request Approval Required - {ticketNumber}";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0d6efd; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .footer {{ background-color: #ffffff; padding: 15px; border: 1px solid #dee2e6; border-top: none; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .info-row {{ margin: 10px 0; }}
        .label {{ font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Access Request Approval Required</h2>
        </div>
        <div class=""content"">
            <p>Dear {manager.FullName},</p>
            <p>An access request has been submitted and requires your approval as the Direct Manager.</p>
            
            <div class=""info-row"">
                <span class=""label"">Ticket Number:</span> {ticketNumber}
            </div>
            <div class=""info-row"">
                <span class=""label"">Employee Name:</span> {accessRequest.FullName}
            </div>
            <div class=""info-row"">
                <span class=""label"">System Name:</span> {accessRequest.SystemName}
            </div>
            <div class=""info-row"">
                <span class=""label"">Access Type:</span> {accessRequest.AccessType}
            </div>
            <div class=""info-row"">
                <span class=""label"">Current Status:</span> Pending Manager Approval
            </div>
            
            <p style=""margin-top: 20px;"">
                <a href=""{approvalUrl}"" class=""button"">Review & Approve Request</a>
            </p>
            
            <p style=""margin-top: 20px; font-size: 0.9em; color: #666;"">
                If the button doesn't work, copy and paste this link into your browser:<br>
                {approvalUrl}
            </p>
        </div>
        <div class=""footer"">
            <p style=""margin: 0; font-size: 0.85em; color: #666;"">
                This is an automated message from the IT Help Desk system. Please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";

            await _emailSender.SendEmailAsync(managerEmail, subject, body);
            _logger.LogInformation("Manager notification sent to {ManagerEmail} for Access Request {TicketId}", managerEmail, ticket.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send manager notification for Access Request {TicketId}", accessRequest.Ticket?.Id);
        }
    }

    public async Task NotifySecurityAsync(AccessRequest accessRequest)
    {
        if (accessRequest?.Ticket == null)
        {
            _logger.LogWarning("Cannot send security notification: AccessRequest or Ticket is null");
            return;
        }

        try
        {
            var ticket = accessRequest.Ticket;
            var ticketNumber = $"HD-{ticket.Id:D6}";
            
            // Find Security user (Mohammed) - typically identified by email containing "mohammed" or FullName starting with "Mohammed"
            // For now, we'll use a default security email or log if not found
            // In production, this should be configured in appsettings or retrieved from user store
            var securityEmail = "mohammed@yub.com.sa"; // TODO: Retrieve from configuration or user store
            
            var approvalUrl = GenerateApprovalUrl(ticket.Id, "ApproveSecurityAccess");
            var subject = $"[IT Help Desk] Security Approval Required - {ticketNumber}";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .footer {{ background-color: #ffffff; padding: 15px; border: 1px solid #dee2e6; border-top: none; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .info-row {{ margin: 10px 0; }}
        .label {{ font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Security Approval Required</h2>
        </div>
        <div class=""content"">
            <p>Dear Security Team,</p>
            <p>An access request has been approved by the Direct Manager and now requires Security approval.</p>
            
            <div class=""info-row"">
                <span class=""label"">Ticket Number:</span> {ticketNumber}
            </div>
            <div class=""info-row"">
                <span class=""label"">Employee Name:</span> {accessRequest.FullName}
            </div>
            <div class=""info-row"">
                <span class=""label"">Manager:</span> {accessRequest.ManagerApprovalName ?? "N/A"}
            </div>
            <div class=""info-row"">
                <span class=""label"">System Name:</span> {accessRequest.SystemName}
            </div>
            <div class=""info-row"">
                <span class=""label"">Access Type:</span> {accessRequest.AccessType}
            </div>
            <div class=""info-row"">
                <span class=""label"">Current Status:</span> Pending Security Approval
            </div>
            
            <p style=""margin-top: 20px;"">
                <a href=""{approvalUrl}"" class=""button"">Review Security Approval</a>
            </p>
            
            <p style=""margin-top: 20px; font-size: 0.9em; color: #666;"">
                If the button doesn't work, copy and paste this link into your browser:<br>
                {approvalUrl}
            </p>
        </div>
        <div class=""footer"">
            <p style=""margin: 0; font-size: 0.85em; color: #666;"">
                This is an automated message from the IT Help Desk system. Please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";

            await _emailSender.SendEmailAsync(securityEmail, subject, body);
            _logger.LogInformation("Security notification sent to {SecurityEmail} for Access Request {TicketId}", securityEmail, ticket.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send security notification for Access Request {TicketId}", accessRequest.Ticket?.Id);
        }
    }

    public async Task NotifyITAsync(AccessRequest accessRequest)
    {
        if (accessRequest?.Ticket == null)
        {
            _logger.LogWarning("Cannot send IT notification: AccessRequest or Ticket is null");
            return;
        }

        try
        {
            var ticket = accessRequest.Ticket;
            var ticketNumber = $"HD-{ticket.Id:D6}";
            
            // Find IT user (Yazan) - typically identified by email containing "yazan"
            var itEmail = "yazan@yub.com.sa"; // TODO: Retrieve from configuration or user store
            
            var executionUrl = GenerateApprovalUrl(ticket.Id, "ExecuteAccessRequest");
            var subject = $"[IT Help Desk] Access Request Ready for Execution - {ticketNumber}";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #198754; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .footer {{ background-color: #ffffff; padding: 15px; border: 1px solid #dee2e6; border-top: none; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #198754; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .info-row {{ margin: 10px 0; }}
        .label {{ font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Access Request Ready for Execution</h2>
        </div>
        <div class=""content"">
            <p>Dear IT Team,</p>
            <p>An access request has been approved by Security and is ready for IT execution.</p>
            
            <div class=""info-row"">
                <span class=""label"">Ticket Number:</span> {ticketNumber}
            </div>
            <div class=""info-row"">
                <span class=""label"">Employee Name:</span> {accessRequest.FullName}
            </div>
            <div class=""info-row"">
                <span class=""label"">System Name:</span> {accessRequest.SystemName}
            </div>
            <div class=""info-row"">
                <span class=""label"">Access Type:</span> {accessRequest.AccessType}
            </div>
            <div class=""info-row"">
                <span class=""label"">Current Status:</span> Pending IT Execution
            </div>
            <div class=""info-row"">
                <span class=""label"">Security Approved By:</span> {accessRequest.SecurityApprovalName ?? "N/A"}
            </div>
            
            <p style=""margin-top: 20px;"">
                <a href=""{executionUrl}"" class=""button"">Execute Request</a>
            </p>
            
            <p style=""margin-top: 20px; font-size: 0.9em; color: #666;"">
                If the button doesn't work, copy and paste this link into your browser:<br>
                {executionUrl}
            </p>
        </div>
        <div class=""footer"">
            <p style=""margin: 0; font-size: 0.85em; color: #666;"">
                This is an automated message from the IT Help Desk system. Please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";

            await _emailSender.SendEmailAsync(itEmail, subject, body);
            _logger.LogInformation("IT notification sent to {ITEmail} for Access Request {TicketId}", itEmail, ticket.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send IT notification for Access Request {TicketId}", accessRequest.Ticket?.Id);
        }
    }

    public async Task NotifyRequestCompletedAsync(AccessRequest accessRequest)
    {
        if (accessRequest?.Ticket == null)
        {
            _logger.LogWarning("Cannot send completion notification: AccessRequest or Ticket is null");
            return;
        }

        try
        {
            var ticket = accessRequest.Ticket;
            var ticketNumber = $"HD-{ticket.Id:D6}";
            var isCompleted = ticket.Status == TicketStatus.Resolved;
            var statusText = isCompleted ? "Completed" : "Closed";
            
            var detailsUrl = GenerateApprovalUrl(ticket.Id, "Details");
            var subject = $"[IT Help Desk] Access Request {statusText} - {ticketNumber}";

            // Determine who to notify
            var employeeEmail = accessRequest.Email;
            var managerEmail = accessRequest.SelectedManager?.Email;
            var securityEmail = "mohammed@yub.com.sa"; // TODO: Retrieve from configuration

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {(isCompleted ? "#198754" : "#6c757d")}; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .footer {{ background-color: #ffffff; padding: 15px; border: 1px solid #dee2e6; border-top: none; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: {(isCompleted ? "#198754" : "#6c757d")}; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .info-row {{ margin: 10px 0; }}
        .label {{ font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Access Request {statusText}</h2>
        </div>
        <div class=""content"">
            <p>Dear {accessRequest.FullName},</p>
            <p>Your access request has been {statusText.ToLower()} by the IT Department.</p>
            
            <div class=""info-row"">
                <span class=""label"">Ticket Number:</span> {ticketNumber}
            </div>
            <div class=""info-row"">
                <span class=""label"">System Name:</span> {accessRequest.SystemName}
            </div>
            <div class=""info-row"">
                <span class=""label"">Access Type:</span> {accessRequest.AccessType}
            </div>
            <div class=""info-row"">
                <span class=""label"">Status:</span> {statusText}
            </div>
            
            <p style=""margin-top: 20px;"">
                <a href=""{detailsUrl}"" class=""button"">View Request Details</a>
            </p>
            
            <p style=""margin-top: 20px; font-size: 0.9em; color: #666;"">
                If the button doesn't work, copy and paste this link into your browser:<br>
                {detailsUrl}
            </p>
        </div>
        <div class=""footer"">
            <p style=""margin: 0; font-size: 0.85em; color: #666;"">
                This is an automated message from the IT Help Desk system. Please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";

            // Send to employee
            if (!string.IsNullOrWhiteSpace(employeeEmail))
            {
                await _emailSender.SendEmailAsync(employeeEmail, subject, body);
                _logger.LogInformation("Completion notification sent to employee {EmployeeEmail} for Access Request {TicketId}", employeeEmail, ticket.Id);
            }

            // Send to manager (optional notification)
            if (!string.IsNullOrWhiteSpace(managerEmail))
            {
                var managerSubject = $"[IT Help Desk] Access Request {statusText} - {ticketNumber} (Manager Notification)";
                await _emailSender.SendEmailAsync(managerEmail, managerSubject, body);
                _logger.LogInformation("Completion notification sent to manager {ManagerEmail} for Access Request {TicketId}", managerEmail, ticket.Id);
            }

            // Send to security (optional notification)
            if (!string.IsNullOrWhiteSpace(securityEmail))
            {
                var securitySubject = $"[IT Help Desk] Access Request {statusText} - {ticketNumber} (Security Notification)";
                await _emailSender.SendEmailAsync(securityEmail, securitySubject, body);
                _logger.LogInformation("Completion notification sent to security {SecurityEmail} for Access Request {TicketId}", securityEmail, ticket.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send completion notification for Access Request {TicketId}", accessRequest.Ticket?.Id);
        }
    }

    private string GenerateApprovalUrl(int ticketId, string action)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var scheme = httpContext.Request.Scheme;
                var host = httpContext.Request.Host;
                var path = $"/Tickets/{action}/{ticketId}";
                return $"{scheme}://{host}{path}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate URL for ticket {TicketId}, action {Action}", ticketId, action);
        }

        // Fallback URL (relative path)
        return $"/Tickets/{action}/{ticketId}";
    }
}

