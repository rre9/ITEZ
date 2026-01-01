using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Http;
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

        // Diagnostic logging: record roles and assigned ticket IDs for troubleshooting
        try
        {
            var roles = string.Join(',', User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));
            var assignedIds = string.Join(',', tickets.Select(t => t.AssignedToId ?? "(null)"));
            _logger.LogInformation("MyTasks diagnostics: user={UserId} roles={Roles} ticketsCount={Count} assignedIds={AssignedIds}", userId, roles, tickets.Count, assignedIds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write MyTasks diagnostic log for user {UserId}", userId);
        }

        // Build review info for access requests
        var reviewInfo = new Dictionary<int, TicketReviewInfo>();
        var accessRequests = await _context.AccessRequests
            .Where(ar => tickets.Select(t => t.Id).Contains(ar.TicketId))
            .ToListAsync();

        foreach (var ticket in tickets)
        {
            var accessRequest = accessRequests.FirstOrDefault(ar => ar.TicketId == ticket.Id);
            if (accessRequest != null)
            {
                // Determine review action based on workflow stage
                // CRITICAL RULE: Past approvers MUST route to Details, never to approval endpoints
                string? reviewAction = null;
                bool canReview = false;

                // Check if user is a past approver (Manager or Security who already approved)
                var isPastManagerApprover = User.IsInRole("Manager") && 
                                           accessRequest.SelectedManagerId == userId &&
                                           accessRequest.ManagerApprovalStatus != ApprovalStatus.Pending;
                var isPastSecurityApprover = User.IsInRole("Security") &&
                                            accessRequest.SecurityApprovalStatus != ApprovalStatus.Pending;

                // Only route to approval endpoints if user is CURRENT approver AND status is Pending
                if (accessRequest.ManagerApprovalStatus == ApprovalStatus.Pending)
                {
                    // Manager approval stage
                    var isCurrentManager = User.IsInRole("Manager") && accessRequest.SelectedManagerId == userId;
                    if (isCurrentManager)
                    {
                        reviewAction = "ApproveAccessRequest";
                        canReview = true;
                    }
                    // Past approvers route to Details (handled below)
                }
                else if (accessRequest.ManagerApprovalStatus == ApprovalStatus.Approved && 
                         accessRequest.SecurityApprovalStatus == ApprovalStatus.Pending)
                {
                    // Security approval stage
                    var isCurrentSecurity = User.IsInRole("Security");
                    if (isCurrentSecurity)
                    {
                        reviewAction = "ApproveSecurityAccess";
                        canReview = true;
                    }
                    // Past approvers route to Details (handled below)
                }
                else if (accessRequest.SecurityApprovalStatus == ApprovalStatus.Approved && 
                         ticket.Status == TicketStatus.InProgress)
                {
                    // IT execution stage
                    var isCurrentIT = User.IsInRole("IT");
                    if (isCurrentIT)
                    {
                        reviewAction = "ExecuteAccessRequest";
                        canReview = true;
                    }
                    // Past approvers route to Details (handled below)
                }

                reviewInfo[ticket.Id] = new TicketReviewInfo
                {
                    CanReview = canReview,
                    ReviewAction = reviewAction
                };
            }
            else if (ticket.Title != null && ticket.Title.StartsWith("System Change Request"))
            {
                // Determine review action for System Change Requests
                string? reviewAction = null;
                bool canReview = false;

                if (ticket.Status == TicketStatus.New)
                {
                    // If assigned manager and user is manager
                    if (User.IsInRole("Manager") && ticket.AssignedToId == userId)
                    {
                        reviewAction = "ApproveSystemChange"; // redirector in TicketsController
                        canReview = true;
                    }
                    else if (User.IsInRole("Security") && ticket.AssignedToId == userId)
                    {
                        reviewAction = "ApproveSecuritySystemChange";
                        canReview = true;
                    }
                }
                else if (ticket.Status == TicketStatus.InProgress)
                {
                    if (User.IsInRole("IT") && ticket.AssignedToId == userId)
                    {
                        reviewAction = "ExecuteSystemChange";
                        canReview = true;
                    }
                }

                reviewInfo[ticket.Id] = new TicketReviewInfo
                {
                    CanReview = canReview,
                    ReviewAction = reviewAction
                };
            }
        }

        var viewModel = new TasksViewModel
        {
            Tickets = tickets,
            ReviewInfo = reviewInfo
        };

        return View(viewModel);
    }

    [Authorize]
    public async Task<IActionResult> TeamRequests()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Allow access for Manager OR Security role
        if (!User.IsInRole("Manager") && !User.IsInRole("Security"))
        {
            return Forbid();
        }

        // Get all access requests where the current user is the selected manager
        var accessRequests = await _context.AccessRequests
            .Where(ar => ar.SelectedManagerId == currentUser.Id)
            .Include(ar => ar.Ticket)
            .Include(ar => ar.SelectedManager)
            .OrderByDescending(ar => ar.CreatedAt)
            .ToListAsync();

        // Include system change tickets assigned to this user (created with title prefix)
        var systemChangeTickets = await _context.Tickets
            .Where(t => t.AssignedToId == currentUser.Id && t.Title.StartsWith("System Change Request"))
            .Include(t => t.CreatedBy)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var vm = new ViewModels.TeamRequestsViewModel
        {
            AccessRequests = accessRequests,
            SystemChangeTickets = systemChangeTickets
        };

        return View(vm);
    }

    // Redirect helper actions to maintain existing links from views
    [Authorize]
    public IActionResult ApproveSystemChange(int id)
    {
        return RedirectToAction("Approve", "SystemChangeRequests", new { id });
    }

    [Authorize]
    public IActionResult ApproveSecuritySystemChange(int id)
    {
        return RedirectToAction("ApproveSecurity", "SystemChangeRequests", new { id });
    }

    [Authorize]
    public IActionResult ExecuteSystemChange(int id)
    {
        return RedirectToAction("Execute", "SystemChangeRequests", new { id });
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
            // Auto-assign new tickets to yazan.it@yub.com.sa for regular employees
            var defaultAssignee = await _userManager.FindByEmailAsync("yazan.it@yub.com.sa");
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

    [Authorize]
    public async Task<IActionResult> SelectRequestType()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> CreateAccessRequest()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        var viewModel = new AccessRequestCreateViewModel
        {
            Departments = _departmentProvider.GetDepartments(),
            Managers = await GetDirectManagersAsync(currentUser.Id)
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccessRequest(AccessRequestCreateViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Repopulate dropdowns if validation fails
        if (!ModelState.IsValid)
        {
            model.Departments = _departmentProvider.GetDepartments();
            model.Managers = await GetDirectManagersAsync(currentUser.Id);
            return View(model);
        }

        // Convert AccessType string to enum
        AccessType accessType;
        switch (model.AccessType?.Trim())
        {
            case "Full Access":
                accessType = AccessType.Full;
                break;
            case "Read Only":
                accessType = AccessType.ReadOnly;
                break;
            case "Edit Access":
                accessType = AccessType.Edit;
                break;
            case "Admin Access":
                accessType = AccessType.Admin;
                break;
            default:
                ModelState.AddModelError(nameof(model.AccessType), "Please select a valid access type.");
                model.Departments = _departmentProvider.GetDepartments();
                model.Managers = await GetDirectManagersAsync(currentUser.Id);
                return View(model);
        }

        // Validate SelectedManagerId exists
        var selectedManager = await _userManager.FindByIdAsync(model.SelectedManagerId);
        if (selectedManager == null)
        {
            ModelState.AddModelError(nameof(model.SelectedManagerId), "Please select a valid manager.");
            model.Departments = _departmentProvider.GetDepartments();
            model.Managers = await GetDirectManagersAsync(currentUser.Id);
            return View(model);
        }

        // Create Ticket
        var ticketTitle = $"Access Request: {model.SystemName} - {model.FullName}";
        var ticketDescription = $"Access Request for {model.FullName} ({model.EmployeeNumber})\n\n" +
                               $"System: {model.SystemName}\n" +
                               $"Access Type: {model.AccessType}\n" +
                               $"Reason: {model.Reason}\n" +
                               $"Start Date: {model.StartDate:yyyy-MM-dd}\n" +
                               (model.EndDate.HasValue ? $"End Date: {model.EndDate.Value:yyyy-MM-dd}\n" : "") +
                               (model.AccessDuration != null ? $"Duration: {model.AccessDuration}\n" : "") +
                               (!string.IsNullOrWhiteSpace(model.Notes) ? $"\nNotes: {model.Notes}" : "");

        var ticket = new Ticket
        {
            Title = ticketTitle,
            Description = ticketDescription.Trim(),
            Department = model.Department.Trim(),
            Priority = TicketPriority.Medium,
            Status = TicketStatus.New,
            CreatedById = currentUser.Id,
            AssignedToId = model.SelectedManagerId, // Assign to selected manager for approval
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        // Create AccessRequest
        var accessRequest = new AccessRequest
        {
            TicketId = ticket.Id,
            FullName = model.FullName.Trim(),
            EmployeeNumber = model.EmployeeNumber.Trim(),
            Department = model.Department.Trim(),
            Email = model.Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
            AccessType = accessType,
            SystemName = model.SystemName.Trim(),
            Reason = model.Reason.Trim(),
            AccessDuration = string.IsNullOrWhiteSpace(model.AccessDuration) ? null : model.AccessDuration.Trim(),
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            SelectedManagerId = model.SelectedManagerId,
            ManagerApprovalStatus = ApprovalStatus.Pending,
            ITApprovalStatus = ApprovalStatus.Pending,
            SecurityApprovalStatus = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();

        // Handle attachments if provided
        if (model.Attachments != null && model.Attachments.Count > 0)
        {
            foreach (var attachment in model.Attachments.Where(f => f is { Length: > 0 }))
            {
                try
                {
                    var metadata = await _attachmentService.SaveAttachmentAsync(ticket.Id, attachment);
                    var ticketAttachment = new TicketAttachment
                    {
                        TicketId = ticket.Id,
                        FileName = metadata.OriginalFileName,
                        FilePath = metadata.RelativePath,
                        UploadTime = metadata.UploadedAt
                    };
                    _context.TicketAttachments.Add(ticketAttachment);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Failed to upload attachment for access request ticket {TicketId}.", ticket.Id);
                    // Continue processing even if attachment fails
                }
            }
        }

        // Create ticket log
        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Access Request Created",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = $"Access request created by {currentUser.FullName}. Assigned to {selectedManager.FullName} for approval."
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "✅ Access request submitted successfully. It has been sent to your manager for approval.";

        // Redirect to MyTickets
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

    [Authorize]
    public async Task<IActionResult> ApproveAccessRequest(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Logs)
                .ThenInclude(l => l.PerformedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var accessRequest = await _context.AccessRequests
            .Include(ar => ar.SelectedManager)
            .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);

        if (accessRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Check if current user is the selected manager (allows past approvers to view forever)
        var isSelectedManager = accessRequest.SelectedManagerId == currentUser.Id;
        var isManager = User.IsInRole("Manager");
        
        // Only allow managers to view (or the selected manager if they're not a manager - edge case)
        if (!isManager && !isSelectedManager)
        {
            return Forbid();
        }

        // IsAuthorizedManager: true if user is the selected manager (regardless of approval status)
        // This allows past approvers to always see their approval details
        var isAuthorizedManager = isSelectedManager && isManager;
        
        // IsReadOnly: true if status is not pending (already approved/rejected)
        // This ensures past approvers see read-only view, but current approvers can act
        var isReadOnly = accessRequest.ManagerApprovalStatus != ApprovalStatus.Pending;

        var viewModel = new AccessRequestApprovalViewModel
        {
            TicketId = ticket.Id,
            Ticket = ticket,
            AccessRequest = accessRequest,
            IsAuthorizedManager = isAuthorizedManager,
            IsReadOnly = isReadOnly,
            Logs = ticket.Logs.OrderByDescending(l => l.Timestamp),
            SelectedManagerName = accessRequest.SelectedManager?.FullName ?? "Unknown"
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveAccessRequest(int id, string? comment)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var accessRequest = await _context.AccessRequests
            .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);

        if (accessRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Verify the current user is the selected manager
        if (accessRequest.SelectedManagerId != currentUser.Id)
        {
            return Forbid();
        }

        // Verify the request is still pending
        if (accessRequest.ManagerApprovalStatus != ApprovalStatus.Pending)
        {
            TempData["Toast"] = "⚠️ This request has already been processed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Check if Mohammed is the ticket creator (skip Security approval)
        var isMohammedCreator = ticket.CreatedById != null && 
            (ticket.CreatedBy?.FullName.StartsWith("Mohammed", StringComparison.OrdinalIgnoreCase) == true ||
             ticket.CreatedBy?.Email?.Contains("mohammed", StringComparison.OrdinalIgnoreCase) == true);

        // Update manager approval
        accessRequest.ManagerApprovalStatus = ApprovalStatus.Approved;
        accessRequest.ManagerApprovalDate = DateTime.UtcNow;
        accessRequest.ManagerApprovalName = currentUser.FullName;

        // Find Security user (Mohammed)
        var securityUser = await _userManager.FindByEmailAsync("mohammed.cyber@yub.com.sa");
        
        if (isMohammedCreator)
        {
            // Skip Security approval - automatically approve and assign to IT
            accessRequest.SecurityApprovalStatus = ApprovalStatus.Approved;
            accessRequest.SecurityApprovalDate = DateTime.UtcNow;
            accessRequest.SecurityApprovalName = securityUser?.FullName ?? "Security (Auto-approved)";

            // Find IT user (Yazan)
            var itUser = await _userManager.FindByEmailAsync("yazan.it@yub.com.sa");
            
            // Assign to IT (Yazan)
            ticket.AssignedToId = itUser?.Id;
            ticket.Status = TicketStatus.InProgress;

            // Add log entry
            var skipLog = new TicketLog
            {
                TicketId = ticket.Id,
                Action = "Security Approval Skipped",
                PerformedById = currentUser.Id,
                Timestamp = DateTime.UtcNow,
                Notes = "Security approval skipped (request created by Security user). Auto-assigned to IT."
            };
            _context.TicketLogs.Add(skipLog);
        }
        else
        {
            // Normal workflow - assign to Security (Mohammed)
            if (securityUser != null)
            {
                ticket.AssignedToId = securityUser.Id;
                ticket.Status = TicketStatus.InProgress;
            }
        }

        // Add approval log
        var logNotes = $"Manager approval granted by {currentUser.FullName}.";
        if (!string.IsNullOrWhiteSpace(comment))
        {
            logNotes += $" Comment: {comment}";
        }
        if (isMohammedCreator)
        {
            logNotes += " Security approval skipped (request created by Security user).";
        }

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Manager Approval",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "✅ Access request approved. Ticket assigned to Security for review.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ApproveSecurityAccess(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Logs)
                .ThenInclude(l => l.PerformedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var accessRequest = await _context.AccessRequests
            .Include(ar => ar.SelectedManager)
            .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);

        if (accessRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Only allow Security role to view (allows past approvers to view forever)
        var isSecurity = User.IsInRole("Security");
        
        if (!isSecurity)
        {
            return Forbid();
        }

        // IsAuthorizedSecurity: true if user is Security (regardless of approval status)
        // This allows past approvers to always see their approval details
        var isAuthorizedSecurity = isSecurity;
        
        // IsReadOnly: true if status is not pending (already approved/rejected)
        // This ensures past approvers see read-only view, but current approvers can act
        var isReadOnly = accessRequest.SecurityApprovalStatus != ApprovalStatus.Pending;
        
        // CanApprove: true if authorized AND status is pending AND manager has approved
        var canApprove = isAuthorizedSecurity && 
                        accessRequest.SecurityApprovalStatus == ApprovalStatus.Pending &&
                        accessRequest.ManagerApprovalStatus == ApprovalStatus.Approved;

        var viewModel = new AccessRequestSecurityApprovalViewModel
        {
            TicketId = ticket.Id,
            Ticket = ticket,
            AccessRequest = accessRequest,
            IsAuthorizedSecurity = isAuthorizedSecurity,
            IsReadOnly = isReadOnly,
            CanApprove = canApprove,
            Logs = ticket.Logs.OrderByDescending(l => l.Timestamp)
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveSecurityAccess(int id, [FromForm] string? comment, [FromForm] List<IFormFile>? attachments)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var accessRequest = await _context.AccessRequests
            .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);

        if (accessRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Verify the current user is Security (Mohammed)
        if (!User.IsInRole("Security"))
        {
            return Forbid();
        }

        // Verify manager has approved
        if (accessRequest.ManagerApprovalStatus != ApprovalStatus.Approved)
        {
            TempData["Toast"] = "⚠️ Manager approval is required before Security review.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Verify the request is still pending Security approval
        if (accessRequest.SecurityApprovalStatus != ApprovalStatus.Pending)
        {
            TempData["Toast"] = "⚠️ This request has already been processed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Update Security approval
        accessRequest.SecurityApprovalStatus = ApprovalStatus.Approved;
        accessRequest.SecurityApprovalDate = DateTime.UtcNow;
        accessRequest.SecurityApprovalName = currentUser.FullName;

        // Find IT user (Yazan)
        var itUser = await _userManager.FindByEmailAsync("yazan.it@yub.com.sa");
        
        // Assign to IT (Yazan)
        ticket.AssignedToId = itUser?.Id;
        ticket.Status = TicketStatus.InProgress;

        // Handle attachments if provided
        if (attachments != null && attachments.Count > 0)
        {
            foreach (var attachment in attachments.Where(f => f is { Length: > 0 }))
            {
                try
                {
                    var metadata = await _attachmentService.SaveAttachmentAsync(ticket.Id, attachment);
                    var ticketAttachment = new TicketAttachment
                    {
                        TicketId = ticket.Id,
                        FileName = metadata.OriginalFileName,
                        FilePath = metadata.RelativePath,
                        UploadTime = metadata.UploadedAt
                    };
                    _context.TicketAttachments.Add(ticketAttachment);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Failed to upload attachment for ticket {TicketId} during Security approval.", ticket.Id);
                }
            }
        }

        // Add approval log
        var logNotes = $"Security approval granted by {currentUser.FullName}.";
        if (!string.IsNullOrWhiteSpace(comment))
        {
            logNotes += $" Comment: {comment}";
        }

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Security Approval",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "✅ Security approval granted. Ticket assigned to IT for execution.";
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

    /// <summary>
    /// Gets the list of Direct Managers (exactly 5 specific users).
    /// Excludes the currently logged-in user.
    /// This is business-based, not role-based.
    /// </summary>
    private async Task<List<UserLookupViewModel>> GetDirectManagersAsync(string? currentUserId)
    {
        // Define the exact 5 Direct Managers by email (business-based list)
        var directManagerEmails = new[]
        {
            "abeer.finance@yub.com.sa",      // Abeer Finance
            "mashael.agg@yub.com.sa",        // Mashael Aggregator
            "mashael.itr@yub.com.sa",        // Mashael IT R
            "mohammed.cyber@yub.com.sa",     // Mohammed Cyber
            "yazan.it@yub.com.sa"            // Yazan IT
        };

        var managers = new List<UserLookupViewModel>();

        foreach (var email in directManagerEmails)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && (currentUserId == null || user.Id != currentUserId))
            {
                managers.Add(new UserLookupViewModel(user.Id, user.FullName, user.Email ?? string.Empty));
            }
        }

        return managers.OrderBy(m => m.FullName).ToList();
    }
}
