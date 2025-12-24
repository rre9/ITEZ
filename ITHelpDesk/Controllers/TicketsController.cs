using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.Services;
using ITHelpDesk.Services.Abstractions;
using ITHelpDesk.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITHelpDesk.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITicketAttachmentService _attachmentService;
    private readonly IEmailSender _emailSender;
    private readonly ITicketQueryService _ticketQueryService;
    private readonly IDepartmentProvider _departmentProvider;
    private readonly ILogger<TicketsController> _logger;
    private readonly IAuthorizationService _authorizationService;

    public TicketsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ITicketAttachmentService attachmentService,
        IEmailSender emailSender,
        ITicketQueryService ticketQueryService,
        IDepartmentProvider departmentProvider,
        ILogger<TicketsController> logger,
        IAuthorizationService authorizationService)
    {
        _context = context;
        _userManager = userManager;
        _attachmentService = attachmentService;
        _emailSender = emailSender;
        _ticketQueryService = ticketQueryService;
        _departmentProvider = departmentProvider;
        _logger = logger;
        _authorizationService = authorizationService;
    }

    [Authorize(Policy = "IsSupportOrAdmin")]
    public async Task<IActionResult> Index([FromQuery] TicketsQuery query)
    {
        query ??= new TicketsQuery();
        var result = await _ticketQueryService.GetTicketsAsync(query);

        var departmentOptions = _departmentProvider.GetDepartments()
            .Select(d => new SelectListItem
            {
                Value = d,
                Text = d,
                Selected = string.Equals(result.Query.Department, d, StringComparison.Ordinal)
            })
            .ToList();

        var statusOptions = Enum.GetValues<TicketStatus>()
            .Select(s => new SelectListItem
            {
                Value = s.ToString(),
                Text = s.ToString(),
                Selected = result.Query.Status == s
            })
            .ToList();

        var priorityOptions = Enum.GetValues<TicketPriority>()
            .Select(p => new SelectListItem
            {
                Value = p.ToString(),
                Text = p.ToString(),
                Selected = result.Query.Priority == p
            })
            .ToList();

        var viewModel = new TicketsIndexViewModel
        {
            Result = result,
            Query = result.Query,
            DepartmentOptions = departmentOptions,
            StatusOptions = statusOptions,
            PriorityOptions = priorityOptions
        };

        return View(viewModel);
    }

    [Authorize(Policy = "IsSupportOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> ExportCsv([FromQuery] TicketsQuery query)
    {
        var tickets = await _ticketQueryService.GetTicketsForExportAsync(query);

        var builder = new StringBuilder();
        builder.AppendLine("TicketNumber,Title,Department,Priority,Status,CreatedAtUtc,Requester,Assignee");

        foreach (var ticket in tickets)
        {
            var ticketNumber = $"HD-{ticket.Id:D6}";
            var requester = ticket.CreatedBy?.FullName ?? string.Empty;
            var assignee = ticket.AssignedTo?.FullName ?? string.Empty;
            builder.AppendLine(string.Join(",",
                Escape(ticketNumber),
                Escape(ticket.Title),
                Escape(ticket.Department),
                Escape(ticket.Priority.ToString()),
                Escape(ticket.Status.ToString()),
                ticket.CreatedAt.ToString("u"),
                Escape(requester),
                Escape(assignee)));
        }

        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        var fileName = $"tickets_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);

        static string Escape(string value)
        {
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }

    public async Task<IActionResult> MyTickets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var tickets = await _context.Tickets
            .Where(t => t.CreatedById == userId)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tickets);
    }

    [Authorize]
    public async Task<IActionResult> MyTasks()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var tickets = await _context.Tickets
            .Where(t => t.AssignedToId == userId)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tickets);
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        var isAdminOrSupport = User.IsInRole("Admin") || User.IsInRole("Support");
        
        var viewModel = new TicketCreateViewModel
        {
            AvailablePriorities = Enum.GetValues<TicketPriority>(),
            AvailableStatuses = Enum.GetValues<TicketStatus>(),
            Departments = _departmentProvider.GetDepartments(),
            CanAssign = isAdminOrSupport
        };

        // Load users list for Admin/Support
        if (isAdminOrSupport)
        {
            viewModel.SupportUsers = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .Select(u => new UserLookupViewModel(u.Id, u.FullName, u.Email ?? string.Empty))
                .ToListAsync();
        }

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketCreateViewModel model)
    {
        var isAdminOrSupport = User.IsInRole("Admin") || User.IsInRole("Support");
        
        if (!ModelState.IsValid)
        {
            model.AvailablePriorities = Enum.GetValues<TicketPriority>();
            model.AvailableStatuses = Enum.GetValues<TicketStatus>();
            model.Departments ??= _departmentProvider.GetDepartments();
            model.CanAssign = isAdminOrSupport;
            if (isAdminOrSupport)
            {
                model.SupportUsers = await _userManager.Users
                    .AsNoTracking()
                    .OrderBy(u => u.FullName)
                    .Select(u => new UserLookupViewModel(u.Id, u.FullName, u.Email ?? string.Empty))
                    .ToListAsync();
            }
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // If Admin/Support assigned someone, use that. Otherwise, auto-assign to yazan@yub.com.sa
        string? assignedToId = null;
        ApplicationUser? assignedToUser = null;
        
        if (isAdminOrSupport && !string.IsNullOrWhiteSpace(model.AssignedToId))
        {
            assignedToId = model.AssignedToId;
            assignedToUser = await _userManager.FindByIdAsync(assignedToId);
        }
        else
        {
            // Auto-assign new tickets to yazan@yub.com.sa for regular employees
            var defaultAssignee = await _userManager.FindByEmailAsync("yazan@yub.com.sa");
            assignedToId = defaultAssignee?.Id;
            assignedToUser = defaultAssignee;
        }

        var ticket = new Ticket
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Department = model.Department.Trim(),
            Priority = model.Priority,
            Status = TicketStatus.New,
            CreatedById = currentUser.Id,
            AssignedToId = assignedToId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        if (model.Attachments is not null && model.Attachments.Count > 0)
        {
            foreach (var attachment in model.Attachments.Where(f => f is { Length: > 0 }))
            {
                try
                {
                    var metadata = await _attachmentService.SaveAttachmentAsync(ticket.Id, attachment);
                    ticket.Attachments.Add(new TicketAttachment
                    {
                        TicketId = ticket.Id,
                        FileName = metadata.OriginalFileName,
                        FilePath = metadata.RelativePath,
                        UploadTime = metadata.UploadedAt
                    });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Attachments", ex.Message);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            model.AvailablePriorities = Enum.GetValues<TicketPriority>();
            model.AvailableStatuses = Enum.GetValues<TicketStatus>();
            model.Departments ??= _departmentProvider.GetDepartments();
            return View(model);
        }

        var logNotes = $"Ticket created by {currentUser.FullName}.";
        if (assignedToId != null && assignedToUser != null)
        {
            if (isAdminOrSupport && !string.IsNullOrWhiteSpace(model.AssignedToId))
            {
                logNotes += $" Assigned to {assignedToUser.FullName}.";
            }
            else
            {
                logNotes += $" Auto-assigned to {assignedToUser.FullName}.";
            }
        }

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Ticket Created",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Send notification to assignee if ticket was assigned
        if (assignedToUser != null && !string.IsNullOrWhiteSpace(assignedToUser.Email) && assignedToUser.Id != currentUser.Id)
        {
            var ticketNumber = $"HD-{ticket.Id:D6}";
            var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
            var assigneeSubject = $"[IT Help Desk] Ticket {ticketNumber} assigned to you";
            var assigneeBody = $"""
<p>Hello {assignedToUser.FullName},</p>
<p>Ticket "{ticket.Title}" ({ticketNumber}) has been assigned to you.</p>
<p>Requester: {currentUser.FullName}<br/>
Department: {ticket.Department}<br/>
Priority: {ticket.Priority}<br/>
Status: {ticket.Status}</p>
<p>View details:<br/>
<a href="{detailsUrl}">{detailsUrl}</a></p>
<p>&mdash; IT Help Desk Team</p>
""";
            try
            {
                await _emailSender.SendEmailAsync(assignedToUser.Email, assigneeSubject, assigneeBody);
                _logger.LogInformation("Assignment notification sent to {Email} for ticket {TicketId}.", assignedToUser.Email, ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send assignment notification to {Email} for ticket {TicketId}.", assignedToUser.Email, ticket.Id);
            }
        }

        TempData["Toast"] = "✅ Ticket created successfully.";
        
        // Redirect based on user role
        if (isAdminOrSupport)
        {
            return RedirectToAction(nameof(MyTasks));
        }
        else
        {
            return RedirectToAction(nameof(MyTickets));
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Attachments)
            .Include(t => t.Logs)
                .ThenInclude(l => l.PerformedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, ticket, "TicketAccess");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        return View(ticket);
    }

    [Authorize]
    public async Task<IActionResult> ChangeStatus(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Attachments)
            .Include(t => t.Logs)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdminOrSupport = User.IsInRole("Admin") || User.IsInRole("Support");
        var isAssignedTo = ticket.AssignedToId == userId;

        // Only Admin/Support or assigned user can update status
        if (!isAdminOrSupport && !isAssignedTo)
        {
            return Forbid();
        }

        // Only Admin/Support can see all users for assignment
        List<UserLookupViewModel> users;
        if (isAdminOrSupport)
        {
            users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .Select(u => new UserLookupViewModel(u.Id, u.FullName, u.Email ?? string.Empty))
                .ToListAsync();
        }
        else
        {
            // Regular employees can only see themselves and Support/Admin users
            var allUsers = await _userManager.Users.AsNoTracking().ToListAsync();
            var filteredUsers = new List<UserLookupViewModel>();
            foreach (var user in allUsers.OrderBy(u => u.FullName))
            {
                if (user.Id == userId || await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Support"))
                {
                    filteredUsers.Add(new UserLookupViewModel(user.Id, user.FullName, user.Email ?? string.Empty));
                }
            }
            users = filteredUsers;
        }

        var model = new TicketStatusUpdateViewModel
        {
            TicketId = ticket.Id,
            CurrentStatus = ticket.Status,
            NewStatus = ticket.Status,
            AssignedToId = ticket.AssignedToId,
            AvailableStatuses = Enum.GetValues<TicketStatus>(),
            SupportUsers = users,
            CanAssign = isAdminOrSupport // Flag to control assignment visibility
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, TicketStatusUpdateViewModel model)
    {
        if (id != model.TicketId)
        {
            return BadRequest();
        }

        var ticket = await _context.Tickets
            .Include(t => t.Logs)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdminOrSupport = User.IsInRole("Admin") || User.IsInRole("Support");
        var isAssignedTo = ticket.AssignedToId == userId;

        // Only Admin/Support or assigned user can update status
        if (!isAdminOrSupport && !isAssignedTo)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.AvailableStatuses = Enum.GetValues<TicketStatus>();
            // Reload users list
            if (isAdminOrSupport)
            {
                model.SupportUsers = await _userManager.Users
                    .AsNoTracking()
                    .OrderBy(u => u.FullName)
                    .Select(u => new UserLookupViewModel(u.Id, u.FullName, u.Email ?? string.Empty))
                    .ToListAsync();
            }
            else
            {
                var allUsers = await _userManager.Users.AsNoTracking().ToListAsync();
                var filteredUsers = new List<UserLookupViewModel>();
                foreach (var user in allUsers.OrderBy(u => u.FullName))
                {
                    if (user.Id == userId || await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Support"))
                    {
                        filteredUsers.Add(new UserLookupViewModel(user.Id, user.FullName, user.Email ?? string.Empty));
                    }
                }
                model.SupportUsers = filteredUsers;
            }
            model.CanAssign = isAdminOrSupport;
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        var originalStatus = ticket.Status;
        var originalAssignee = ticket.AssignedToId;
        var originalAssigneeUser = ticket.AssignedTo;

        ticket.Status = model.NewStatus;
        ticket.AssignedToId = string.IsNullOrWhiteSpace(model.AssignedToId) ? null : model.AssignedToId;

        // Load new assignee if changed
        ApplicationUser? newAssigneeUser = null;
        if (ticket.AssignedToId != null && ticket.AssignedToId != originalAssignee)
        {
            newAssigneeUser = await _userManager.FindByIdAsync(ticket.AssignedToId);
        }

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Status Update",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = CreateStatusChangeNotes(originalStatus, ticket.Status, originalAssignee, ticket.AssignedToId, model.InternalNotes)
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Handle file attachment if provided
        if (model.Attachment is not null && model.Attachment.Length > 0)
        {
            try
            {
                var metadata = await _attachmentService.SaveAttachmentAsync(ticket.Id, model.Attachment);
                var attachment = new TicketAttachment
                {
                    TicketId = ticket.Id,
                    FileName = metadata.OriginalFileName,
                    FilePath = metadata.RelativePath,
                    UploadTime = metadata.UploadedAt
                };
                _context.TicketAttachments.Add(attachment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Attachment uploaded for ticket {TicketId} during status update.", ticket.Id);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to upload attachment for ticket {TicketId} during status update.", ticket.Id);
                // Don't fail the status update if attachment fails, just log it
            }
        }

        // Reload ticket with new assignee
        await _context.Entry(ticket).Reference(t => t.AssignedTo).LoadAsync();

        string toastMessage;

        // Send notification to new assignee if ticket was transferred
        if (newAssigneeUser != null && !string.IsNullOrWhiteSpace(newAssigneeUser.Email))
        {
            var ticketNumber = $"HD-{ticket.Id:D6}";
            var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
            var assigneeSubject = $"[IT Help Desk] Ticket {ticketNumber} assigned to you";
            var assigneeBody = $"""
<p>Hello {newAssigneeUser.FullName},</p>
<p>Ticket "{ticket.Title}" ({ticketNumber}) has been assigned to you.</p>
<p>Requester: {ticket.CreatedBy?.FullName}<br/>
Department: {ticket.Department}<br/>
Priority: {ticket.Priority}<br/>
Status: {ticket.Status}</p>
<p>View details:<br/>
<a href="{detailsUrl}">{detailsUrl}</a></p>
<p>&mdash; IT Help Desk Team</p>
""";
            try
            {
                await _emailSender.SendEmailAsync(newAssigneeUser.Email, assigneeSubject, assigneeBody);
                _logger.LogInformation("Assignment notification sent to {Email} for ticket {TicketId}.", newAssigneeUser.Email, ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send assignment notification to {Email} for ticket {TicketId}.", newAssigneeUser.Email, ticket.Id);
            }
        }

        if (string.IsNullOrWhiteSpace(ticket.CreatedBy?.Email))
        {
            _logger.LogWarning("Ticket {TicketId} has no requester email. Skipping notification.", ticket.Id);
            toastMessage = "⚠️ Ticket status updated, but the requester email is missing.";
        }
        else
        {
            var ticketNumber = $"HD-{ticket.Id:D6}";
            var assignedDisplay = ticket.AssignedTo?.FullName ?? "Unassigned";
            var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
            var subject = $"[IT Help Desk] Ticket {ticketNumber} status changed to {ticket.Status}";

            var body = $"""
<p>Hello {ticket.CreatedBy.FullName},</p>
<p>Your IT Help Desk ticket "{ticket.Title}" ({ticketNumber}) has been updated.</p>
<p>Previous status: {originalStatus}<br/>
New status: {ticket.Status}<br/>
Assigned To: {assignedDisplay}</p>
<p>View details:<br/>
<a href="{detailsUrl}">{detailsUrl}</a></p>
<p>&mdash; IT Help Desk Team</p>
""";

            try
            {
                await _emailSender.SendEmailAsync(ticket.CreatedBy.Email, subject, body);
                toastMessage = "✅ Ticket status updated and the requester has been notified.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send status update email for ticket {TicketId}.", ticket.Id);
                toastMessage = "⚠️ Ticket status updated, but the requester could not be notified by email.";
            }
        }

        TempData["Toast"] = toastMessage;
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    private static string CreateStatusChangeNotes(
        TicketStatus originalStatus,
        TicketStatus newStatus,
        string? originalAssigneeId,
        string? newAssigneeId,
        string? internalNotes)
    {
        var changes = $"Status changed from {originalStatus} to {newStatus}.";

        if (!string.Equals(originalAssigneeId, newAssigneeId, StringComparison.Ordinal))
        {
            changes += $" Assignment updated.";
        }

        if (!string.IsNullOrWhiteSpace(internalNotes))
        {
            changes += $" Notes: {internalNotes.Trim()}";
        }

        return changes;
    }
}
