using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.Services;
using ITHelpDesk.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
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
    private readonly ILogger<TicketsController> _logger;
    private readonly IDepartmentProvider _departmentProvider;

    public TicketsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ITicketAttachmentService attachmentService,
        IEmailSender emailSender,
        ILogger<TicketsController> logger,
        IDepartmentProvider departmentProvider)
    {
        _context = context;
        _userManager = userManager;
        _attachmentService = attachmentService;
        _emailSender = emailSender;
        _logger = logger;
        _departmentProvider = departmentProvider;
    }

    [Authorize(Policy = "IsSupportOrAdmin")]
    public async Task<IActionResult> Index()
    {
        var tickets = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tickets);
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

    public async Task<IActionResult> Create()
    {
        var viewModel = new TicketCreateViewModel
        {
            AvailablePriorities = Enum.GetValues<TicketPriority>(),
            AvailableStatuses = Enum.GetValues<TicketStatus>(),
            Departments = _departmentProvider.GetDepartments()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailablePriorities = Enum.GetValues<TicketPriority>();
            model.AvailableStatuses = Enum.GetValues<TicketStatus>();
            model.Departments = _departmentProvider.GetDepartments();
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        var ticket = new Ticket
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Department = model.Department.Trim(),
            Priority = model.Priority,
            Status = TicketStatus.New,
            CreatedById = currentUser.Id,
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
            model.Departments = _departmentProvider.GetDepartments();
            return View(model);
        }

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Ticket Created",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = $"Ticket created by {currentUser.FullName}."
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Ticket created successfully.";
        return RedirectToAction(nameof(MyTickets));
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

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdminOrSupport = User.IsInRole("Admin") || User.IsInRole("Support");

        if (!isAdminOrSupport && ticket.CreatedById != userId)
        {
            return Forbid();
        }

        return View(ticket);
    }

    [Authorize(Policy = "IsSupportOrAdmin")]
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

        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new UserLookupViewModel(u.Id, u.FullName, u.Email ?? string.Empty))
            .ToListAsync();

        var model = new TicketStatusUpdateViewModel
        {
            TicketId = ticket.Id,
            CurrentStatus = ticket.Status,
            NewStatus = ticket.Status,
            AssignedToId = ticket.AssignedToId,
            AvailableStatuses = Enum.GetValues<TicketStatus>(),
            SupportUsers = users
        };

        return View(model);
    }

    [Authorize(Policy = "IsSupportOrAdmin")]
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

        if (!ModelState.IsValid)
        {
            model.AvailableStatuses = Enum.GetValues<TicketStatus>();
            model.SupportUsers = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .Select(u => new UserLookupViewModel(u.Id, u.FullName, u.Email ?? string.Empty))
                .ToListAsync();
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        var originalStatus = ticket.Status;
        var originalAssignee = ticket.AssignedToId;

        ticket.Status = model.NewStatus;
        ticket.AssignedToId = string.IsNullOrWhiteSpace(model.AssignedToId) ? null : model.AssignedToId;

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

        string toastMessage;

        if (string.IsNullOrWhiteSpace(ticket.CreatedBy?.Email))
        {
            _logger.LogWarning("Ticket {TicketId} has no requester email. Skipping notification.", ticket.Id);
            toastMessage = "⚠️ Ticket status updated, but the requester email is missing.";
        }
        else
        {
            var assignedDisplay = ticket.AssignedTo?.FullName ?? "Unassigned";
            var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
            var subject = $"[IT Help Desk] Ticket #{ticket.Id} status changed to {ticket.Status}";

            var body = $@"
<p><strong>Arabic:</strong></p>
<p>مرحبًا {ticket.CreatedBy.FullName},</p>
<p>تم تحديث حالة تذكرتك ""{ticket.Title}"".</p>
<p>الحالة السابقة: {originalStatus}<br/>
الحالة الجديدة: {ticket.Status}<br/>
المكلّف: {assignedDisplay}</p>
<p>للاطّلاع على التفاصيل:<br/>
<a href=""{detailsUrl}"">{detailsUrl}</a></p>
<p>فريق الدعم الفني – IT Help Desk</p>
<hr/>
<p><strong>English:</strong></p>
<p>Hello {ticket.CreatedBy.FullName},</p>
<p>Your IT Help Desk ticket ""{ticket.Title}"" has been updated.</p>
<p>Previous status: {originalStatus}<br/>
New status: {ticket.Status}<br/>
Assigned To: {assignedDisplay}</p>
<p>View details:<br/>
<a href=""{detailsUrl}"">{detailsUrl}</a></p>
<p>&mdash; IT Help Desk Team</p>";

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
            changes += " Assignment updated.";
        }

        if (!string.IsNullOrWhiteSpace(internalNotes))
        {
            changes += $" Notes: {internalNotes.Trim()}";
        }

        return changes;
    }
}

