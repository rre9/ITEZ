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
using ITHelpDesk.Services.Notifications;
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
    private readonly INotificationService _notificationService;

    public TicketsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ITicketAttachmentService attachmentService,
        IEmailSender emailSender,
        ITicketQueryService ticketQueryService,
        IDepartmentProvider departmentProvider,
        ILogger<TicketsController> logger,
        IAuthorizationService authorizationService,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _attachmentService = attachmentService;
        _emailSender = emailSender;
        _ticketQueryService = ticketQueryService;
        _departmentProvider = departmentProvider;
        _logger = logger;
        _authorizationService = authorizationService;
        _notificationService = notificationService;
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
        var isIT = User.IsInRole("IT");
        var currentUser = await _userManager.GetUserAsync(User);
        
        // Log for debugging
        _logger.LogInformation(
            "MyTasks called - UserId: {UserId}, Email: {Email}, IsIT: {IsIT}",
            userId, currentUser?.Email, isIT);

        IQueryable<Ticket> ticketsQuery;

        if (isIT)
        {
            // 🔥 CRITICAL FIX: Get ALL IT users and check tickets assigned to ANY of them
            // This ensures we find tickets even if assigned to different IT user
            
            var allITUsers = await _userManager.GetUsersInRoleAsync("IT");
            var itUserIds = allITUsers.Select(u => u.Id).ToList();
            
            _logger.LogInformation(
                "MyTasks IT - Current User: Id={CurrentUserId}, Email={Email}. " +
                "Total IT users in system: {TotalITUsers}. IT User IDs: {ITUserIds}",
                userId, currentUser?.Email, allITUsers.Count, string.Join(", ", itUserIds));
            
            // Check ALL tickets assigned to ANY IT user (any status)
            var allAssignedTickets = await _context.Tickets
                .Where(t => t.AssignedToId != null && itUserIds.Contains(t.AssignedToId))
                .Select(t => new { t.Id, t.Title, t.Status, t.AssignedToId })
                .ToListAsync();
            
            _logger.LogInformation(
                "MyTasks IT - Found {Count} tickets assigned to ANY IT user (any status): {Tickets}",
                allAssignedTickets.Count,
                string.Join(", ", allAssignedTickets.Select(t => $"#{t.Id} (AssignedTo: {t.AssignedToId}, Status: {t.Status})")));
            
            // 🔥 CRITICAL: Show tickets assigned to ANY IT user (including closed tickets for documentation)
            // This ensures we find tickets even if assigned to different IT user ID
            ticketsQuery = _context.Tickets
                .Where(t => t.AssignedToId != null && 
                           itUserIds.Contains(t.AssignedToId))
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo);
            
            _logger.LogInformation(
                "MyTasks IT - Query: AssignedToId IN ({ITUserIds}) (all statuses including closed). " +
                "This will show tickets assigned to ANY IT user.",
                string.Join(", ", itUserIds));
        }
        else
        {
            // For non-IT users: Show tickets assigned to user (including closed tickets for documentation)
            ticketsQuery = _context.Tickets
                .Where(t => t.AssignedToId == userId)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo);
        }

        var tickets = await ticketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
        
        _logger.LogInformation(
            "MyTasks - Final result: {Count} tickets returned for user {UserId} (Email: {Email})",
            tickets.Count, userId, currentUser?.Email);
        
        // 🚨 DEBUG: Log each ticket for IT users
        if (isIT && tickets.Count > 0)
        {
            foreach (var t in tickets)
            {
                _logger.LogInformation(
                    "MyTasks IT - Ticket {TicketId}: Title={Title}, AssignedToId={AssignedToId}, Status={Status}",
                    t.Id, t.Title, t.AssignedToId, t.Status);
            }
        }
        else if (isIT && tickets.Count == 0)
        {
            // 🔥 CRITICAL DEBUG: Check database directly for IT stage tickets
            var allITUsers = await _userManager.GetUsersInRoleAsync("IT");
            var itUserIds = allITUsers.Select(u => u.Id).ToList();
            
            // Check ALL tickets assigned to ANY IT user with Status InProgress
            var dbTickets = await _context.Tickets
                .Where(t => t.AssignedToId != null && 
                           itUserIds.Contains(t.AssignedToId) &&
                           t.Status == TicketStatus.InProgress)
                .Select(t => new { t.Id, t.Title, t.Status, t.AssignedToId })
                .ToListAsync();
            
            // Check Access Requests for these tickets
            var accessRequestTickets = await _context.AccessRequests
                .Where(ar => dbTickets.Select(t => t.Id).Contains(ar.TicketId))
                .Select(ar => new { 
                    TicketId = ar.TicketId, 
                    ManagerStatus = ar.ManagerApprovalStatus, 
                    SecurityStatus = ar.SecurityApprovalStatus 
                })
                .ToListAsync();
            
            _logger.LogWarning(
                "MyTasks IT - No tickets found in query result. " +
                "Direct DB check: Found {Count} tickets assigned to IT users with Status=InProgress. " +
                "Tickets: {Tickets}. " +
                "Access Requests: {AccessRequests}",
                dbTickets.Count, 
                string.Join(", ", dbTickets.Select(t => $"#{t.Id} (AssignedTo: {t.AssignedToId}, Status: {t.Status})")),
                string.Join(", ", accessRequestTickets.Select(ar => $"Ticket {ar.TicketId}: Manager={ar.ManagerStatus}, Security={ar.SecurityStatus}")));
        }

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
        // Build review info for access requests (only for remaining tickets after IT filtering)
        var reviewInfo = new Dictionary<int, TicketReviewInfo>();
        var accessRequestsForReview = await _context.AccessRequests
            .Where(ar => tickets.Select(t => t.Id).Contains(ar.TicketId))
            .ToListAsync();
        
        var serviceRequestsForReview = await _context.ServiceRequests
            .Where(sr => tickets.Select(t => t.Id).Contains(sr.TicketId))
            .ToListAsync();

        foreach (var ticket in tickets)
        {
            // If ticket is closed, only show View button (no Review button)
            if (ticket.Status == TicketStatus.Closed)
            {
                reviewInfo[ticket.Id] = new TicketReviewInfo
                {
                    CanReview = false,
                    ReviewAction = null
                };
                continue;
            }
            
            var accessRequest = accessRequestsForReview.FirstOrDefault(ar => ar.TicketId == ticket.Id);
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
                    // Security can review if ticket is assigned to them
                    var isCurrentSecurity = User.IsInRole("Security");
                    var isAssignedToSecurity = ticket.AssignedToId == userId;
                    if (isCurrentSecurity && isAssignedToSecurity)
                    {
                        reviewAction = "ApproveSecurityAccess";
                        canReview = true;
                    }
                    // Past approvers route to Details (handled below)
                }
                // IT stage: IT users should NOT see Review button
                // They should only see View button and use Update Status from Details
                // No reviewAction for IT - they use ChangeStatus instead

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
            else
            {
                // Check if this is a Service Request
                var serviceRequest = serviceRequestsForReview.FirstOrDefault(sr => sr.TicketId == ticket.Id);
                if (serviceRequest != null)
                {
                    string? reviewAction = null;
                    bool canReview = false;

                    // Determine review action based on workflow stage
                    if (serviceRequest.ManagerApprovalStatus == ApprovalStatus.Pending)
                    {
                        // Manager approval stage
                        var isCurrentManager = User.IsInRole("Manager") && serviceRequest.SelectedManagerId == userId;
                        if (isCurrentManager)
                        {
                            reviewAction = "ApproveServiceRequest"; // Manager approval
                            canReview = true;
                        }
                    }
                    else if (serviceRequest.ManagerApprovalStatus == ApprovalStatus.Approved && 
                             serviceRequest.SecurityApprovalStatus == ApprovalStatus.Pending)
                    {
                        // Security approval stage
                        // Security can review if ticket is assigned to them
                        var isCurrentSecurity = User.IsInRole("Security");
                        var isAssignedToSecurity = ticket.AssignedToId == userId;
                        if (isCurrentSecurity && isAssignedToSecurity)
                        {
                            reviewAction = "ApproveSecurityService";
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

        // Get service requests where the current user is the selected manager
        var serviceRequests = await _context.ServiceRequests
            .Where(sr => sr.SelectedManagerId == currentUser.Id)
            .Include(sr => sr.Ticket)
            .Include(sr => sr.SelectedManager)
            .OrderByDescending(sr => sr.RequestDate)
            .ToListAsync();

        var vm = new ViewModels.TeamRequestsViewModel
        {
            AccessRequests = accessRequests,
            SystemChangeTickets = systemChangeTickets,
            ServiceRequests = serviceRequests
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

    [Authorize]
    public async Task<IActionResult> CreateServiceRequest()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        var viewModel = new ServiceRequestCreateViewModel
        {
            EmployeeName = currentUser.FullName,
            Departments = _departmentProvider.GetDepartments(),
            Managers = await GetDirectManagersAsync(currentUser.Id),
            RequestDate = DateTime.UtcNow,
            SignatureDate = DateTime.UtcNow,
            SignatureName = currentUser.FullName
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateServiceRequest(ServiceRequestCreateViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Validate Acknowledged checkbox
        if (!model.Acknowledged)
        {
            ModelState.AddModelError(nameof(model.Acknowledged), "You must acknowledge and agree to the responsibility terms to submit this request.");
        }

        // Repopulate dropdowns if validation fails
        if (!ModelState.IsValid)
        {
            model.Departments = _departmentProvider.GetDepartments();
            model.Managers = await GetDirectManagersAsync(currentUser.Id);
            if (string.IsNullOrWhiteSpace(model.EmployeeName))
            {
                model.EmployeeName = currentUser.FullName;
            }
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
        var ticketTitle = $"Service Request - Social Media Access Permit: {model.EmployeeName}";
        var ticketDescription = $"Service Request - Social Media Access Permit for {model.EmployeeName}\n\n" +
                               $"Department: {model.Department}\n" +
                               $"Job Title: {model.JobTitle}\n" +
                               $"Request Date: {model.RequestDate:yyyy-MM-dd}\n\n" +
                               $"Usage Description: {model.UsageDescription}\n\n" +
                               $"Reason: {model.UsageReason}\n\n" +
                               $"Signature: {model.SignatureName} ({model.SignatureJobTitle})\n" +
                               $"Signature Date: {model.SignatureDate:yyyy-MM-dd}";

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

        // Create ServiceRequest
        var serviceRequest = new ServiceRequest
        {
            TicketId = ticket.Id,
            EmployeeName = model.EmployeeName.Trim(),
            Department = model.Department.Trim(),
            JobTitle = model.JobTitle.Trim(),
            RequestDate = model.RequestDate,
            UsageDescription = model.UsageDescription.Trim(),
            UsageReason = model.UsageReason.Trim(),
            Acknowledged = model.Acknowledged,
            SignatureName = model.SignatureName.Trim(),
            SignatureJobTitle = model.SignatureJobTitle.Trim(),
            SignatureDate = model.SignatureDate,
            SelectedManagerId = model.SelectedManagerId,
            ManagerApprovalStatus = ApprovalStatus.Pending,
            ITApprovalStatus = ApprovalStatus.Pending,
            SecurityApprovalStatus = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequests.Add(serviceRequest);
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
                    _logger.LogWarning(ex, "Failed to upload attachment for service request ticket {TicketId}.", ticket.Id);
                    // Continue processing even if attachment fails
                }
            }
        }

        // Create ticket log
        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Service Request Created",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = $"Service request created by {currentUser.FullName}. Assigned to {selectedManager.FullName} for approval."
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Load SelectedManager for notification
        await _context.Entry(serviceRequest).Reference(sr => sr.SelectedManager).LoadAsync();
        await _context.Entry(serviceRequest).Reference(sr => sr.Ticket).LoadAsync();

        // Send notification to manager
        await _notificationService.NotifyServiceRequestManagerAsync(serviceRequest);

        TempData["Toast"] = "✅ Service request submitted successfully. It has been sent to your manager for approval.";

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

        // Check if this is an Access/Service Request in IT Execution stage for IT user
        var isIT = User.IsInRole("IT");
        var currentUser = await _userManager.GetUserAsync(User);
        var canITExecute = false;
        AccessRequest? accessRequestForIT = null;
        ServiceRequest? serviceRequestForIT = null;
        
        if (isIT && ticket.Status == TicketStatus.InProgress && ticket.AssignedToId == currentUser?.Id)
        {
            accessRequestForIT = await _context.AccessRequests
                .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);
            
            serviceRequestForIT = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);
            
            // IT Execution stage requirements:
            // 1. User Role = IT
            // 2. Manager has approved
            // 3. Security has approved
            // 4. AssignedTo = Current User
            // 5. Status = InProgress
            var isAccessRequestInITStage = accessRequestForIT != null &&
                                          accessRequestForIT.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                          accessRequestForIT.SecurityApprovalStatus == ApprovalStatus.Approved;
            
            var isServiceRequestInITStage = serviceRequestForIT != null &&
                                           serviceRequestForIT.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                           serviceRequestForIT.SecurityApprovalStatus == ApprovalStatus.Approved;
            
            canITExecute = (isAccessRequestInITStage || isServiceRequestInITStage) &&
                          ticket.Status == TicketStatus.InProgress &&
                          ticket.AssignedToId == currentUser.Id;
        }

        ViewBag.CanITExecute = canITExecute;
        ViewBag.IsIT = isIT;
        ViewBag.AccessRequestForIT = accessRequestForIT;
        ViewBag.ServiceRequestForIT = serviceRequestForIT;

        return View(ticket);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteAndClose(int id, [FromForm] string? notes)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Verify user is IT and ticket is assigned to them
        if (!User.IsInRole("IT") || ticket.AssignedToId != currentUser.Id)
        {
            return Forbid();
        }

        // Verify ticket is in IT Execution stage
        if (ticket.Status != TicketStatus.InProgress)
        {
            TempData["Toast"] = "⚠️ This ticket is not in IT Execution stage.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Verify Manager and Security have approved
        var accessRequest = await _context.AccessRequests
            .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);
        
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        var isAccessRequestInITStage = accessRequest != null &&
                                      accessRequest.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                      accessRequest.SecurityApprovalStatus == ApprovalStatus.Approved;
        
        var isServiceRequestInITStage = serviceRequest != null &&
                                       serviceRequest.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                       serviceRequest.SecurityApprovalStatus == ApprovalStatus.Approved;

        if (!isAccessRequestInITStage && !isServiceRequestInITStage)
        {
            TempData["Toast"] = "⚠️ Manager and Security approvals are required before IT Execution.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Validate notes
        if (string.IsNullOrWhiteSpace(notes))
        {
            TempData["Toast"] = "⚠️ Completion notes are required.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Update ticket status to Closed
        ticket.Status = TicketStatus.Closed;
        ticket.CloseReason = CloseReason.Completed;
        // Keep AssignedToId for documentation (ticket remains visible in Tasks)

        // Update IT approval status
        if (isAccessRequestInITStage && accessRequest != null)
        {
            accessRequest.ITApprovalStatus = ApprovalStatus.Approved;
            accessRequest.ITApprovalDate = DateTime.UtcNow;
            accessRequest.ITApprovalName = currentUser.FullName;
        }
        else if (isServiceRequestInITStage && serviceRequest != null)
        {
            serviceRequest.ITApprovalStatus = ApprovalStatus.Approved;
            serviceRequest.ITApprovalDate = DateTime.UtcNow;
            serviceRequest.ITApprovalName = currentUser.FullName;
        }

        // Add completion log
        var logNotes = $"IT Completed and Closed Request by {currentUser.FullName}. Notes: {notes}";

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "IT Completed and Closed Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "✅ Request completed and closed successfully.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectAndClose(int id, [FromForm] string? notes)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Verify user is IT and ticket is assigned to them
        if (!User.IsInRole("IT") || ticket.AssignedToId != currentUser.Id)
        {
            return Forbid();
        }

        // Verify ticket is in IT Execution stage
        if (ticket.Status != TicketStatus.InProgress)
        {
            TempData["Toast"] = "⚠️ This ticket is not in IT Execution stage.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Verify Manager and Security have approved
        var accessRequest = await _context.AccessRequests
            .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);
        
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        var isAccessRequestInITStage = accessRequest != null &&
                                      accessRequest.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                      accessRequest.SecurityApprovalStatus == ApprovalStatus.Approved;
        
        var isServiceRequestInITStage = serviceRequest != null &&
                                       serviceRequest.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                       serviceRequest.SecurityApprovalStatus == ApprovalStatus.Approved;

        if (!isAccessRequestInITStage && !isServiceRequestInITStage)
        {
            TempData["Toast"] = "⚠️ Manager and Security approvals are required before IT Execution.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Validate notes
        if (string.IsNullOrWhiteSpace(notes))
        {
            TempData["Toast"] = "⚠️ Rejection reason is required.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Update ticket status to Closed
        ticket.Status = TicketStatus.Closed;
        ticket.CloseReason = CloseReason.Rejected;
        // Keep AssignedToId for documentation (ticket remains visible in Tasks)

        // Update IT approval status
        if (isAccessRequestInITStage && accessRequest != null)
        {
            accessRequest.ITApprovalStatus = ApprovalStatus.Rejected;
            accessRequest.ITApprovalDate = DateTime.UtcNow;
            accessRequest.ITApprovalName = currentUser.FullName;
        }
        else if (isServiceRequestInITStage && serviceRequest != null)
        {
            serviceRequest.ITApprovalStatus = ApprovalStatus.Rejected;
            serviceRequest.ITApprovalDate = DateTime.UtcNow;
            serviceRequest.ITApprovalName = currentUser.FullName;
        }

        // Add rejection log
        var logNotes = $"IT Rejected and Closed Request by {currentUser.FullName}. Reason: {notes}";

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "IT Rejected and Closed Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "⚠️ Request rejected and closed.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
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
        var isIT = User.IsInRole("IT");
        var isAssignedTo = ticket.AssignedToId == userId;

        // Check if ticket is already closed (Resolved or Rejected) - prevent further updates
        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Rejected)
        {
            TempData["Toast"] = "⚠️ This ticket has been closed and cannot be updated.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Check if this is an Access Request in IT stage
        var accessRequest = await _context.AccessRequests
            .Include(ar => ar.SelectedManager)
            .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);
        
        // CRITICAL: IT stage requires BOTH Manager AND Security approval
        // Any rejection in any stage must stop the workflow
        var isAccessRequestInITStage = accessRequest != null &&
                                       accessRequest.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                       accessRequest.SecurityApprovalStatus == ApprovalStatus.Approved &&
                                       ticket.Status == TicketStatus.InProgress &&
                                       isIT;

        // Check if this is a Service Request in IT stage
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.SelectedManager)
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);
        
        // CRITICAL: IT stage requires BOTH Manager AND Security approval
        // Any rejection in any stage must stop the workflow
        var isServiceRequestInITStage = serviceRequest != null &&
                                       serviceRequest.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                       serviceRequest.SecurityApprovalStatus == ApprovalStatus.Approved &&
                                       ticket.Status == TicketStatus.InProgress &&
                                       isIT;

        // Authorization: Only Admin/Support, IT (for IT stage requests), or assigned user can update status
        if (!isAdminOrSupport && !isIT && !isAssignedTo)
        {
            return Forbid();
        }

        // For IT users: Only allow if this is an Access/Service Request in IT stage
        if (isIT && !isAccessRequestInITStage && !isServiceRequestInITStage)
        {
            return Forbid();
        }

        // Determine available statuses and users based on context
        IEnumerable<TicketStatus> availableStatuses;
        List<UserLookupViewModel> users;
        bool canAssign;

        if (isAccessRequestInITStage || isServiceRequestInITStage)
        {
            // IT stage for Access/Service Request: Final decision - Only Resolved and Rejected options
            // No assignment allowed - this is a final decision stage
            availableStatuses = new[] { TicketStatus.Resolved, TicketStatus.Rejected };
            canAssign = false;
            users = new List<UserLookupViewModel>(); // Empty list since no assignment is allowed
        }
        else if (isAdminOrSupport)
        {
            // Admin/Support: All statuses and all users
            availableStatuses = Enum.GetValues<TicketStatus>();
            users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .Select(u => new UserLookupViewModel(u.Id, u.FullName, u.Email ?? string.Empty))
                .ToListAsync();
            canAssign = true;
        }
        else
        {
            // Regular employees: All statuses but limited user selection
            availableStatuses = Enum.GetValues<TicketStatus>();
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
            canAssign = false;
        }

        var model = new TicketStatusUpdateViewModel
        {
            TicketId = ticket.Id,
            CurrentStatus = ticket.Status,
            NewStatus = ticket.Status,
            AssignedToId = ticket.AssignedToId,
            AvailableStatuses = availableStatuses,
            SupportUsers = users,
            CanAssign = canAssign,
            RequireComment = isAccessRequestInITStage || isServiceRequestInITStage // Comment is required for IT final decision
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
        var isIT = User.IsInRole("IT");
        var isAssignedTo = ticket.AssignedToId == userId;

        // Check if ticket is already closed (Resolved or Rejected) - prevent further updates
        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Rejected)
        {
            TempData["Toast"] = "⚠️ This ticket has been closed and cannot be updated.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Check if this is an Access Request in IT stage
        var accessRequest = await _context.AccessRequests
            .Include(ar => ar.SelectedManager)
            .Include(ar => ar.Ticket)
            .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);
        
        // CRITICAL: IT stage requires BOTH Manager AND Security approval
        // Any rejection in any stage must stop the workflow
        var isAccessRequestInITStage = accessRequest != null &&
                                       accessRequest.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                       accessRequest.SecurityApprovalStatus == ApprovalStatus.Approved &&
                                       ticket.Status == TicketStatus.InProgress &&
                                       isIT;

        // Check if this is a Service Request in IT stage
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.SelectedManager)
            .Include(sr => sr.Ticket)
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);
        
        // CRITICAL: IT stage requires BOTH Manager AND Security approval
        // Any rejection in any stage must stop the workflow
        var isServiceRequestInITStage = serviceRequest != null &&
                                        serviceRequest.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                        serviceRequest.SecurityApprovalStatus == ApprovalStatus.Approved &&
                                        ticket.Status == TicketStatus.InProgress &&
                                        isIT;

        // Authorization: Only Admin/Support, IT (for IT stage requests), or assigned user can update status
        if (!isAdminOrSupport && !isIT && !isAssignedTo)
        {
            return Forbid();
        }

        // For IT users: Only allow if this is an Access/Service Request in IT stage
        if (isIT && !isAccessRequestInITStage && !isServiceRequestInITStage)
        {
            return Forbid();
        }

        // Validate required comment for IT final decision
        if ((isAccessRequestInITStage || isServiceRequestInITStage) && string.IsNullOrWhiteSpace(model.InternalNotes))
        {
            ModelState.AddModelError(nameof(model.InternalNotes), "Comment is required for final decision (Resolved or Rejected).");
        }

        if (!ModelState.IsValid)
        {
            // Reload model with same logic as GET
            IEnumerable<TicketStatus> availableStatuses;
            List<UserLookupViewModel> users;
            bool canAssign;

            if (isAccessRequestInITStage || isServiceRequestInITStage)
            {
                availableStatuses = new[] { TicketStatus.Resolved, TicketStatus.Rejected };
                canAssign = false;
                users = new List<UserLookupViewModel>(); // Empty list since no assignment is allowed
            }
            else if (isAdminOrSupport)
            {
                availableStatuses = Enum.GetValues<TicketStatus>();
                users = await _userManager.Users
                    .AsNoTracking()
                    .OrderBy(u => u.FullName)
                    .Select(u => new UserLookupViewModel(u.Id, u.FullName, u.Email ?? string.Empty))
                    .ToListAsync();
                canAssign = true;
            }
            else
            {
                availableStatuses = Enum.GetValues<TicketStatus>();
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
                canAssign = false;
            }

            model.AvailableStatuses = availableStatuses;
            model.SupportUsers = users;
            model.CanAssign = canAssign;
            model.RequireComment = isAccessRequestInITStage || isServiceRequestInITStage;
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
        
        // For Access/Service Request in IT stage: Final decision - no assignment allowed
        if (isAccessRequestInITStage || isServiceRequestInITStage)
        {
            // IT final decision: Unassign ticket (no assignment needed)
            ticket.AssignedToId = null;
            
            // Update IT approval status based on decision
            if (isAccessRequestInITStage && accessRequest != null)
            {
                accessRequest.ITApprovalStatus = model.NewStatus == TicketStatus.Resolved 
                    ? ApprovalStatus.Approved 
                    : ApprovalStatus.Rejected;
                accessRequest.ITApprovalDate = DateTime.UtcNow;
                accessRequest.ITApprovalName = currentUser.FullName;
            }
            else if (isServiceRequestInITStage && serviceRequest != null)
            {
                serviceRequest.ITApprovalStatus = model.NewStatus == TicketStatus.Resolved 
                    ? ApprovalStatus.Approved 
                    : ApprovalStatus.Rejected;
                serviceRequest.ITApprovalDate = DateTime.UtcNow;
                serviceRequest.ITApprovalName = currentUser.FullName;
            }
        }
        else
        {
            ticket.AssignedToId = string.IsNullOrWhiteSpace(model.AssignedToId) ? null : model.AssignedToId;
        }

        // Create log entry
        string logNotes;
        if (isAccessRequestInITStage)
        {
            if (model.NewStatus == TicketStatus.Resolved)
            {
                logNotes = $"Access Request completed by IT ({currentUser.FullName}).";
            }
            else if (model.NewStatus == TicketStatus.Rejected)
            {
                logNotes = $"Access Request rejected by IT ({currentUser.FullName}).";
            }
            else
            {
                logNotes = $"Access Request status changed by IT ({currentUser.FullName}).";
            }
            
            if (!string.IsNullOrWhiteSpace(model.InternalNotes))
            {
                logNotes += $" Comment: {model.InternalNotes}";
            }
        }
        else if (isServiceRequestInITStage)
        {
            if (model.NewStatus == TicketStatus.Resolved)
            {
                logNotes = $"Service Request completed by IT ({currentUser.FullName}).";
            }
            else if (model.NewStatus == TicketStatus.Rejected)
            {
                logNotes = $"Service Request rejected by IT ({currentUser.FullName}).";
            }
            else
            {
                logNotes = $"Service Request status changed by IT ({currentUser.FullName}).";
            }
            
            if (!string.IsNullOrWhiteSpace(model.InternalNotes))
            {
                logNotes += $" Comment: {model.InternalNotes}";
            }
        }
        else
        {
            logNotes = CreateStatusChangeNotes(originalStatus, ticket.Status, originalAssignee, ticket.AssignedToId, model.InternalNotes);
            if (!string.IsNullOrWhiteSpace(model.InternalNotes))
            {
                logNotes += $" Notes: {model.InternalNotes}";
            }
        }

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = isAccessRequestInITStage 
                ? (model.NewStatus == TicketStatus.Resolved ? "Access Request Completed" : 
                   model.NewStatus == TicketStatus.Rejected ? "Access Request Rejected" : "Status Update")
                : isServiceRequestInITStage
                ? (model.NewStatus == TicketStatus.Resolved ? "Service Request Completed" : 
                   model.NewStatus == TicketStatus.Rejected ? "Service Request Rejected" : "Status Update")
                : "Status Update",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
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

        // Reload ticket with new assignee and AccessRequest/ServiceRequest if needed
        await _context.Entry(ticket).Reference(t => t.AssignedTo).LoadAsync();
        
        // Reload AccessRequest if this was an Access Request in IT stage
        if (isAccessRequestInITStage && accessRequest != null)
        {
            await _context.Entry(accessRequest).Reference(ar => ar.Ticket).LoadAsync();
            await _context.Entry(accessRequest).Reference(ar => ar.SelectedManager).LoadAsync();
        }
        
        // Reload ServiceRequest if this was a Service Request in IT stage
        if (isServiceRequestInITStage && serviceRequest != null)
        {
            await _context.Entry(serviceRequest).Reference(sr => sr.Ticket).LoadAsync();
            await _context.Entry(serviceRequest).Reference(sr => sr.SelectedManager).LoadAsync();
        }

        string toastMessage;

        // Handle Access Request IT stage final decision (Resolved or Rejected)
        if (isAccessRequestInITStage && accessRequest != null)
        {
            var ticketNumber = $"HD-{ticket.Id:D6}";
            var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
            
            if (model.NewStatus == TicketStatus.Resolved)
            {
                // Access Request completed - send notifications to all parties
                await _notificationService.NotifyRequestCompletedAsync(accessRequest);
                toastMessage = "✅ Access Request completed successfully. Notifications sent to employee, manager, and security.";
            }
            else if (model.NewStatus == TicketStatus.Rejected)
            {
                // Access Request rejected - send notifications to all parties
                var subject = $"[IT Help Desk] Access Request Rejected - {ticketNumber}";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Access Request Rejected</h2>
        </div>
        <div class=""content"">
            <p>Dear recipient,</p>
            <p>The Access Request {ticketNumber} has been rejected by IT Department.</p>
            <p><strong>Employee:</strong> {accessRequest.FullName}</p>
            <p><strong>System:</strong> {accessRequest.SystemName}</p>
            <p><strong>Rejected by:</strong> {currentUser.FullName}</p>
            {(string.IsNullOrWhiteSpace(model.InternalNotes) ? "" : $"<p><strong>Reason:</strong> {model.InternalNotes}</p>")}
            <p><a href=""{detailsUrl}"" class=""button"">View Request Details</a></p>
        </div>
    </div>
</body>
</html>";

                // Send notification to employee
                if (!string.IsNullOrWhiteSpace(accessRequest.Email))
                {
                    await _emailSender.SendEmailAsync(accessRequest.Email, subject, body);
                    _logger.LogInformation("Rejection notification sent to employee {Email} for Access Request {TicketId}", accessRequest.Email, ticket.Id);
                }

                // Send notification to manager
                if (accessRequest.SelectedManager != null && !string.IsNullOrWhiteSpace(accessRequest.SelectedManager.Email))
                {
                    await _emailSender.SendEmailAsync(accessRequest.SelectedManager.Email, subject, body);
                    _logger.LogInformation("Rejection notification sent to manager {Email} for Access Request {TicketId}", accessRequest.SelectedManager.Email, ticket.Id);
                }

                // Send notification to Security (Mohammed)
                var securityUser = await _userManager.FindByEmailAsync("mohammed.cyber@yub.com.sa");
                if (securityUser != null && !string.IsNullOrWhiteSpace(securityUser.Email))
                {
                    await _emailSender.SendEmailAsync(securityUser.Email, subject, body);
                    _logger.LogInformation("Rejection notification sent to Security {Email} for Access Request {TicketId}", securityUser.Email, ticket.Id);
                }

                toastMessage = "⚠️ Access Request rejected. Notifications sent to employee, manager, and security.";
            }
            else
            {
                toastMessage = "✅ Status updated successfully.";
            }
        }
        // Handle Service Request IT stage final decision (Resolved or Rejected)
        else if (isServiceRequestInITStage && serviceRequest != null)
        {
            var ticketNumber = $"HD-{ticket.Id:D6}";
            var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
            
            if (model.NewStatus == TicketStatus.Resolved)
            {
                // Service Request completed - send notifications to all parties
                await _notificationService.NotifyServiceRequestCompletedAsync(serviceRequest);
                toastMessage = "✅ Service Request completed successfully. Notifications sent to employee, manager, and security.";
            }
            else if (model.NewStatus == TicketStatus.Rejected)
            {
                // Service Request rejected - send notifications to all parties
                var subject = $"[IT Help Desk] Service Request Rejected - {ticketNumber}";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Service Request Rejected</h2>
        </div>
        <div class=""content"">
            <p>Dear recipient,</p>
            <p>The Service Request {ticketNumber} has been rejected by IT Department.</p>
            <p><strong>Employee:</strong> {serviceRequest.EmployeeName}</p>
            <p><strong>Department:</strong> {serviceRequest.Department}</p>
            <p><strong>Rejected by:</strong> {currentUser.FullName}</p>
            {(string.IsNullOrWhiteSpace(model.InternalNotes) ? "" : $"<p><strong>Reason:</strong> {model.InternalNotes}</p>")}
            <p><a href=""{detailsUrl}"" class=""button"">View Request Details</a></p>
        </div>
    </div>
</body>
</html>";

                // Send notification to employee (ticket creator)
                if (ticket.CreatedBy != null && !string.IsNullOrWhiteSpace(ticket.CreatedBy.Email))
                {
                    await _emailSender.SendEmailAsync(ticket.CreatedBy.Email, subject, body);
                    _logger.LogInformation("Rejection notification sent to employee {Email} for Service Request {TicketId}", ticket.CreatedBy.Email, ticket.Id);
                }

                // Send notification to manager
                if (serviceRequest.SelectedManager != null && !string.IsNullOrWhiteSpace(serviceRequest.SelectedManager.Email))
                {
                    await _emailSender.SendEmailAsync(serviceRequest.SelectedManager.Email, subject, body);
                    _logger.LogInformation("Rejection notification sent to manager {Email} for Service Request {TicketId}", serviceRequest.SelectedManager.Email, ticket.Id);
                }

                // Send notification to Security (Mohammed)
                var securityUser = await _userManager.FindByEmailAsync("mohammed.cyber@yub.com.sa");
                if (securityUser != null && !string.IsNullOrWhiteSpace(securityUser.Email))
                {
                    await _emailSender.SendEmailAsync(securityUser.Email, subject, body);
                    _logger.LogInformation("Rejection notification sent to Security {Email} for Service Request {TicketId}", securityUser.Email, ticket.Id);
                }

                toastMessage = "⚠️ Service Request rejected. Notifications sent to employee, manager, and security.";
            }
            else
            {
                toastMessage = "✅ Status updated successfully.";
            }
        }
        else
        {
            // Load new assignee if changed (for non-IT stage tickets)
            ApplicationUser? newAssigneeUser = null;
            if (ticket.AssignedToId != null && ticket.AssignedToId != originalAssignee)
            {
                newAssigneeUser = await _userManager.FindByIdAsync(ticket.AssignedToId);
            }
            
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

        // CRITICAL: Verify ticket is not already rejected
        if (ticket.Status == TicketStatus.Rejected)
        {
            TempData["Toast"] = "⚠️ This ticket has been rejected and cannot proceed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
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
            // CRITICAL: Only assign to IT if Manager has approved (not rejected)
            // This ensures rejected requests never reach IT
            if (accessRequest.ManagerApprovalStatus == ApprovalStatus.Approved)
            {
                // Skip Security approval - automatically approve and assign to IT
                accessRequest.SecurityApprovalStatus = ApprovalStatus.Approved;
                accessRequest.SecurityApprovalDate = DateTime.UtcNow;
                accessRequest.SecurityApprovalName = securityUser?.FullName ?? "Security (Auto-approved)";

                // Find IT user (Yazan)
                var itUser = await _userManager.FindByEmailAsync("yazan.it@yub.com.sa");
                
                if (itUser == null)
                {
                    _logger.LogError("IT user (yazan.it@yub.com.sa) not found. Cannot assign ticket {TicketId} to IT.", ticket.Id);
                    TempData["Toast"] = "⚠️ IT user not found. Please contact administrator.";
                    return RedirectToAction(nameof(Details), new { id = ticket.Id });
                }
                
                // Assign to IT (Yazan) - ONLY if Manager approved
                ticket.AssignedToId = itUser.Id;
                ticket.Status = TicketStatus.InProgress;
            }

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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectAccessRequest(int id, string? comment)
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

        // Validate comment is required for rejection
        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["Toast"] = "⚠️ A rejection reason is required.";
            return RedirectToAction(nameof(ApproveAccessRequest), new { id = ticket.Id });
        }

        // Update manager approval to Rejected
        accessRequest.ManagerApprovalStatus = ApprovalStatus.Rejected;
        accessRequest.ManagerApprovalDate = DateTime.UtcNow;
        accessRequest.ManagerApprovalName = currentUser.FullName;

        // Close the ticket - final rejection
        ticket.Status = TicketStatus.Rejected;
        ticket.AssignedToId = null; // Unassign - ticket is closed

        // Add rejection log
        var logNotes = $"Manager rejection by {currentUser.FullName}. Reason: {comment}";

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Manager Rejected Access Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Send rejection notifications
        var ticketNumber = $"HD-{ticket.Id:D6}";
        var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
        var subject = $"[IT Help Desk] Access Request Rejected by Manager - {ticketNumber}";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Access Request Rejected</h2>
        </div>
        <div class=""content"">
            <p>Dear recipient,</p>
            <p>The Access Request {ticketNumber} has been rejected by your Direct Manager.</p>
            <p><strong>Employee:</strong> {accessRequest.FullName}</p>
            <p><strong>System:</strong> {accessRequest.SystemName}</p>
            <p><strong>Rejected by:</strong> {currentUser.FullName}</p>
            <p><strong>Reason:</strong> {comment}</p>
            <p><a href=""{detailsUrl}"" class=""button"">View Request Details</a></p>
        </div>
    </div>
</body>
</html>";

        // Send notification to employee
        if (!string.IsNullOrWhiteSpace(accessRequest.Email))
        {
            await _emailSender.SendEmailAsync(accessRequest.Email, subject, body);
            _logger.LogInformation("Rejection notification sent to employee {Email} for Access Request {TicketId}", accessRequest.Email, ticket.Id);
        }

        TempData["Toast"] = "⚠️ Access request rejected. The ticket has been closed and the employee has been notified.";
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

        // SECURITY CAN REVIEW TICKETS ASSIGNED TO THEM
        // This is required for the Security approval workflow stage
        // Security must be able to review tickets assigned to them after Manager approval

        // IsAuthorizedSecurity: true if user is Security (regardless of approval status)
        // This allows past approvers to always see their approval details
        var isAuthorizedSecurity = isSecurity;
        
        // IsReadOnly: true if status is not pending (already approved/rejected)
        // This ensures past approvers see read-only view, but current approvers can act
        var isReadOnly = accessRequest.SecurityApprovalStatus != ApprovalStatus.Pending;
        
        // CanApprove: true if authorized AND status is pending AND manager has approved
        // AND ticket is assigned to current Security user (for Security approval stage)
        var canApprove = isAuthorizedSecurity && 
                        accessRequest.SecurityApprovalStatus == ApprovalStatus.Pending &&
                        accessRequest.ManagerApprovalStatus == ApprovalStatus.Approved &&
                        ticket.AssignedToId == currentUser.Id; // Security must be assigned to approve

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

        // SECURITY CAN REVIEW TICKETS ASSIGNED TO THEM
        // This is required for the Security approval workflow stage
        // Security must be able to review tickets assigned to them after Manager approval
        // Only allow if ticket is assigned to current Security user (for Security approval stage)
        if (ticket.AssignedToId != currentUser.Id)
        {
            return Forbid();
        }

        // CRITICAL: Verify ticket is not already rejected
        if (ticket.Status == TicketStatus.Rejected)
        {
            TempData["Toast"] = "⚠️ This ticket has been rejected and cannot proceed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // CRITICAL: Verify manager has approved (not rejected)
        if (accessRequest.ManagerApprovalStatus != ApprovalStatus.Approved)
        {
            if (accessRequest.ManagerApprovalStatus == ApprovalStatus.Rejected)
            {
                TempData["Toast"] = "⚠️ This request has been rejected by the manager and cannot proceed.";
            }
            else
            {
                TempData["Toast"] = "⚠️ Manager approval is required before Security review.";
            }
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Verify the request is still pending Security approval
        if (accessRequest.SecurityApprovalStatus != ApprovalStatus.Pending)
        {
            TempData["Toast"] = "⚠️ This request has already been processed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // CRITICAL: Final check before assigning to IT
        // Ensure Manager has approved (not rejected) and ticket is not rejected
        if (accessRequest.ManagerApprovalStatus != ApprovalStatus.Approved || 
            ticket.Status == TicketStatus.Rejected)
        {
            TempData["Toast"] = "⚠️ Cannot assign to IT: Manager approval required and ticket must not be rejected.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Update Security approval
        accessRequest.SecurityApprovalStatus = ApprovalStatus.Approved;
        accessRequest.SecurityApprovalDate = DateTime.UtcNow;
        accessRequest.SecurityApprovalName = currentUser.FullName;

        _logger.LogInformation(
            "BEFORE IT Assignment - Ticket {TicketId}: ManagerStatus={ManagerStatus}, SecurityStatus={SecurityStatus}, TicketStatus={TicketStatus}, AssignedToId={AssignedToId}",
            ticket.Id, accessRequest.ManagerApprovalStatus, accessRequest.SecurityApprovalStatus, ticket.Status, ticket.AssignedToId);

        // 🔥 CRITICAL FIX: Find ALL IT users (not just by email)
        // This ensures we assign to the correct IT user regardless of email
        var itUsers = await _userManager.GetUsersInRoleAsync("IT");
        
        if (itUsers == null || itUsers.Count == 0)
        {
            _logger.LogError("No IT users found in database. Cannot assign ticket {TicketId} to IT.", ticket.Id);
            TempData["Toast"] = "⚠️ IT user not found. Please contact administrator.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }
        
        // Use the first IT user (or find by email if multiple)
        var itUser = itUsers.FirstOrDefault(u => u.Email?.Contains("yazan", StringComparison.OrdinalIgnoreCase) == true) 
                     ?? itUsers.FirstOrDefault();
        
        if (itUser == null)
        {
            _logger.LogError("IT user not found. Cannot assign ticket {TicketId} to IT.", ticket.Id);
            TempData["Toast"] = "⚠️ IT user not found. Please contact administrator.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }
        
        _logger.LogInformation(
            "IT User found: Id={ITUserId}, Email={ITUserEmail}, FullName={ITUserFullName}. Total IT users: {TotalITUsers}",
            itUser.Id, itUser.Email, itUser.FullName, itUsers.Count);
        
        // CRITICAL: Assign to IT ONLY if all conditions are met
        // ManagerApprovalStatus == Approved AND SecurityApprovalStatus == Approved (just set above)
        var oldAssignedToId = ticket.AssignedToId;
        ticket.AssignedToId = itUser.Id;
        ticket.Status = TicketStatus.InProgress;
        
        // Add Security approval log entry for documentation and tracking
        var logNotes = $"Security approval granted by {currentUser.FullName}.";
        if (!string.IsNullOrWhiteSpace(comment))
        {
            logNotes += $" Comment: {comment}";
        }
        logNotes += $" Ticket assigned to {itUser.FullName} for IT execution.";

        var approvalLog = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Security Approved Access Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(approvalLog);
        
        _logger.LogInformation(
            "BEFORE SaveChangesAsync - Ticket {TicketId}: OldAssignedToId={OldId}, NewAssignedToId={NewId}, Status={Status}, SecurityStatus={SecurityStatus}",
            ticket.Id, oldAssignedToId, ticket.AssignedToId, ticket.Status, accessRequest.SecurityApprovalStatus);
        
        // Save all changes including the log entry
        var savedChanges = await _context.SaveChangesAsync();
        
        _logger.LogInformation(
            "AFTER SaveChangesAsync - Ticket {TicketId}: SavedChanges={SavedChanges}, AssignedToId={AssignedToId}, Status={Status}, SecurityStatus={SecurityStatus}",
            ticket.Id, savedChanges, ticket.AssignedToId, ticket.Status, accessRequest.SecurityApprovalStatus);
        
        // Verify from database directly
        var verifyTicket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);
        
        var verifyAccessRequest = await _context.AccessRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(ar => ar.TicketId == ticket.Id);
        
        // Verify Security Approval log entry was saved
        var verifyLog = await _context.TicketLogs
            .AsNoTracking()
            .Where(l => l.TicketId == ticket.Id && l.Action == "Security Approved Access Request")
            .OrderByDescending(l => l.Timestamp)
            .FirstOrDefaultAsync();
        
        _logger.LogInformation(
            "VERIFICATION FROM DB - Ticket {TicketId}: AssignedToId={AssignedToId}, Status={Status}, ManagerStatus={ManagerStatus}, SecurityStatus={SecurityStatus}, SecurityLogExists={LogExists}",
            ticket.Id, verifyTicket?.AssignedToId, verifyTicket?.Status, 
            verifyAccessRequest?.ManagerApprovalStatus, verifyAccessRequest?.SecurityApprovalStatus,
            verifyLog != null);
        
        // Direct redirect to MyTasks to verify assignment
        return RedirectToAction(nameof(MyTasks));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectSecurityAccess(int id, [FromForm] string? comment)
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

        // Verify the current user is Security (Mohammed)
        if (!User.IsInRole("Security"))
        {
            return Forbid();
        }

        // SECURITY CAN REVIEW TICKETS ASSIGNED TO THEM
        // This is required for the Security approval workflow stage
        // Security must be able to review tickets assigned to them after Manager approval
        // Only allow if ticket is assigned to current Security user (for Security approval stage)
        if (ticket.AssignedToId != currentUser.Id)
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

        // Validate comment is required for rejection
        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["Toast"] = "⚠️ A rejection reason is required.";
            return RedirectToAction(nameof(ApproveSecurityAccess), new { id = ticket.Id });
        }

        // Update Security approval to Rejected
        accessRequest.SecurityApprovalStatus = ApprovalStatus.Rejected;
        accessRequest.SecurityApprovalDate = DateTime.UtcNow;
        accessRequest.SecurityApprovalName = currentUser.FullName;

        // Close the ticket - final rejection
        ticket.Status = TicketStatus.Rejected;
        ticket.AssignedToId = null; // Unassign - ticket is closed

        // Add rejection log
        var logNotes = $"Security rejection by {currentUser.FullName}. Reason: {comment}";

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Security Rejected Access Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Send rejection notifications
        var ticketNumber = $"HD-{ticket.Id:D6}";
        var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
        var subject = $"[IT Help Desk] Access Request Rejected by Security - {ticketNumber}";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Access Request Rejected</h2>
        </div>
        <div class=""content"">
            <p>Dear recipient,</p>
            <p>The Access Request {ticketNumber} has been rejected by Security Department.</p>
            <p><strong>Employee:</strong> {accessRequest.FullName}</p>
            <p><strong>System:</strong> {accessRequest.SystemName}</p>
            <p><strong>Rejected by:</strong> {currentUser.FullName}</p>
            <p><strong>Reason:</strong> {comment}</p>
            <p><a href=""{detailsUrl}"" class=""button"">View Request Details</a></p>
        </div>
    </div>
</body>
</html>";

        // Send notification to employee
        if (!string.IsNullOrWhiteSpace(accessRequest.Email))
        {
            await _emailSender.SendEmailAsync(accessRequest.Email, subject, body);
            _logger.LogInformation("Rejection notification sent to employee {Email} for Access Request {TicketId}", accessRequest.Email, ticket.Id);
        }

        // Send notification to manager
        if (accessRequest.SelectedManager != null && !string.IsNullOrWhiteSpace(accessRequest.SelectedManager.Email))
        {
            await _emailSender.SendEmailAsync(accessRequest.SelectedManager.Email, subject, body);
            _logger.LogInformation("Rejection notification sent to manager {Email} for Access Request {TicketId}", accessRequest.SelectedManager.Email, ticket.Id);
        }

        TempData["Toast"] = "⚠️ Access request rejected. The ticket has been closed and notifications have been sent to employee and manager.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ApproveServiceRequest(int id)
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

        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.SelectedManager)
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        if (serviceRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Check if current user is the selected manager (allows past approvers to view forever)
        var isSelectedManager = serviceRequest.SelectedManagerId == currentUser.Id;
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
        var isReadOnly = serviceRequest.ManagerApprovalStatus != ApprovalStatus.Pending;

        var viewModel = new ServiceRequestApprovalViewModel
        {
            TicketId = ticket.Id,
            Ticket = ticket,
            ServiceRequest = serviceRequest,
            IsAuthorizedManager = isAuthorizedManager,
            IsReadOnly = isReadOnly,
            Logs = ticket.Logs.OrderByDescending(l => l.Timestamp),
            SelectedManagerName = serviceRequest.SelectedManager?.FullName ?? "Unknown"
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveServiceRequest(int id, string? comment)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        if (serviceRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Verify the current user is the selected manager
        if (serviceRequest.SelectedManagerId != currentUser.Id)
        {
            return Forbid();
        }

        // CRITICAL: Verify ticket is not already rejected
        if (ticket.Status == TicketStatus.Rejected)
        {
            TempData["Toast"] = "⚠️ This ticket has been rejected and cannot proceed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Verify the request is still pending
        if (serviceRequest.ManagerApprovalStatus != ApprovalStatus.Pending)
        {
            TempData["Toast"] = "⚠️ This request has already been processed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Check if Mohammed is the ticket creator (skip Security approval)
        var isMohammedCreator = ticket.CreatedById != null && 
            (ticket.CreatedBy?.FullName.StartsWith("Mohammed", StringComparison.OrdinalIgnoreCase) == true ||
             ticket.CreatedBy?.Email?.Contains("mohammed", StringComparison.OrdinalIgnoreCase) == true);

        // Update manager approval
        serviceRequest.ManagerApprovalStatus = ApprovalStatus.Approved;
        serviceRequest.ManagerApprovalDate = DateTime.UtcNow;
        serviceRequest.ManagerApprovalName = currentUser.FullName;

        // Find Security user (Mohammed)
        var securityUser = await _userManager.FindByEmailAsync("mohammed.cyber@yub.com.sa");
        
        if (isMohammedCreator)
        {
            // CRITICAL: Only assign to IT if Manager has approved (not rejected)
            // This ensures rejected requests never reach IT
            if (serviceRequest.ManagerApprovalStatus == ApprovalStatus.Approved)
            {
                // Skip Security approval - automatically approve and assign to IT
                serviceRequest.SecurityApprovalStatus = ApprovalStatus.Approved;
                serviceRequest.SecurityApprovalDate = DateTime.UtcNow;
                serviceRequest.SecurityApprovalName = securityUser?.FullName ?? "Security (Auto-approved)";

                // Find IT user (Yazan)
                var itUser = await _userManager.FindByEmailAsync("yazan.it@yub.com.sa");
                
                if (itUser == null)
                {
                    _logger.LogError("IT user (yazan.it@yub.com.sa) not found. Cannot assign ticket {TicketId} to IT.", ticket.Id);
                    TempData["Toast"] = "⚠️ IT user not found. Please contact administrator.";
                    return RedirectToAction(nameof(Details), new { id = ticket.Id });
                }
                
                // Assign to IT (Yazan) - ONLY if Manager approved
                ticket.AssignedToId = itUser.Id;
                ticket.Status = TicketStatus.InProgress;
            }

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

        // Load for notification
        await _context.Entry(serviceRequest).Reference(sr => sr.Ticket).LoadAsync();
        
        if (isMohammedCreator)
        {
            // Send notification to IT directly (Security skipped)
            await _notificationService.NotifyServiceRequestITAsync(serviceRequest);
        }
        else
        {
            // Send notification to Security
            await _notificationService.NotifyServiceRequestSecurityAsync(serviceRequest);
        }

        TempData["Toast"] = "✅ Service request approved. Ticket assigned to Security for review.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectServiceRequest(int id, string? comment)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.SelectedManager)
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        if (serviceRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Verify the current user is the selected manager
        if (serviceRequest.SelectedManagerId != currentUser.Id)
        {
            return Forbid();
        }

        // Verify the request is still pending
        if (serviceRequest.ManagerApprovalStatus != ApprovalStatus.Pending)
        {
            TempData["Toast"] = "⚠️ This request has already been processed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Validate comment is required for rejection
        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["Toast"] = "⚠️ A rejection reason is required.";
            return RedirectToAction(nameof(ApproveServiceRequest), new { id = ticket.Id });
        }

        // Update manager approval to Rejected
        serviceRequest.ManagerApprovalStatus = ApprovalStatus.Rejected;
        serviceRequest.ManagerApprovalDate = DateTime.UtcNow;
        serviceRequest.ManagerApprovalName = currentUser.FullName;

        // Close the ticket - final rejection
        ticket.Status = TicketStatus.Rejected;
        ticket.AssignedToId = null; // Unassign - ticket is closed

        // Add rejection log
        var logNotes = $"Manager rejection by {currentUser.FullName}. Reason: {comment}";

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Manager Rejected Service Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Send rejection notifications
        var ticketNumber = $"HD-{ticket.Id:D6}";
        var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
        var subject = $"[IT Help Desk] Service Request Rejected by Manager - {ticketNumber}";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Service Request Rejected</h2>
        </div>
        <div class=""content"">
            <p>Dear recipient,</p>
            <p>The Service Request {ticketNumber} has been rejected by your Direct Manager.</p>
            <p><strong>Employee:</strong> {serviceRequest.EmployeeName}</p>
            <p><strong>Department:</strong> {serviceRequest.Department}</p>
            <p><strong>Rejected by:</strong> {currentUser.FullName}</p>
            <p><strong>Reason:</strong> {comment}</p>
            <p><a href=""{detailsUrl}"" class=""button"">View Request Details</a></p>
        </div>
    </div>
</body>
</html>";

        // Send notification to employee (ticket creator)
        if (ticket.CreatedBy != null && !string.IsNullOrWhiteSpace(ticket.CreatedBy.Email))
        {
            await _emailSender.SendEmailAsync(ticket.CreatedBy.Email, subject, body);
            _logger.LogInformation("Rejection notification sent to employee {Email} for Service Request {TicketId}", ticket.CreatedBy.Email, ticket.Id);
        }

        TempData["Toast"] = "⚠️ Service request rejected. The ticket has been closed and the employee has been notified.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ApproveSecurityService(int id)
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

        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.SelectedManager)
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        if (serviceRequest is null)
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

        // SECURITY CAN REVIEW TICKETS ASSIGNED TO THEM
        // This is required for the Security approval workflow stage
        // Security must be able to review tickets assigned to them after Manager approval

        // IsAuthorizedSecurity: true if user is Security (regardless of approval status)
        // This allows past approvers to always see their approval details
        var isAuthorizedSecurity = isSecurity;
        
        // IsReadOnly: true if status is not pending (already approved/rejected)
        // This ensures past approvers see read-only view, but current approvers can act
        var isReadOnly = serviceRequest.SecurityApprovalStatus != ApprovalStatus.Pending;
        
        // CanApprove: true if authorized AND status is pending AND manager has approved
        var canApprove = isAuthorizedSecurity && 
                        serviceRequest.SecurityApprovalStatus == ApprovalStatus.Pending &&
                        serviceRequest.ManagerApprovalStatus == ApprovalStatus.Approved;

        var viewModel = new ServiceRequestSecurityApprovalViewModel
        {
            TicketId = ticket.Id,
            Ticket = ticket,
            ServiceRequest = serviceRequest,
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
    public async Task<IActionResult> ApproveSecurityService(int id, [FromForm] string? comment, [FromForm] List<IFormFile>? attachments)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        if (serviceRequest is null)
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

        // SECURITY CAN REVIEW TICKETS ASSIGNED TO THEM
        // This is required for the Security approval workflow stage
        // Security must be able to review tickets assigned to them after Manager approval
        // Only allow if ticket is assigned to current Security user (for Security approval stage)
        if (ticket.AssignedToId != currentUser.Id)
        {
            return Forbid();
        }

        // CRITICAL: Verify ticket is not already rejected
        if (ticket.Status == TicketStatus.Rejected)
        {
            TempData["Toast"] = "⚠️ This ticket has been rejected and cannot proceed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // CRITICAL: Verify manager has approved (not rejected)
        if (serviceRequest.ManagerApprovalStatus != ApprovalStatus.Approved)
        {
            if (serviceRequest.ManagerApprovalStatus == ApprovalStatus.Rejected)
            {
                TempData["Toast"] = "⚠️ This request has been rejected by the manager and cannot proceed.";
            }
            else
            {
                TempData["Toast"] = "⚠️ Manager approval is required before Security review.";
            }
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Verify the request is still pending Security approval
        if (serviceRequest.SecurityApprovalStatus != ApprovalStatus.Pending)
        {
            TempData["Toast"] = "⚠️ This request has already been processed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // CRITICAL: Final check before assigning to IT
        // Ensure Manager has approved (not rejected) and ticket is not rejected
        if (serviceRequest.ManagerApprovalStatus != ApprovalStatus.Approved || 
            ticket.Status == TicketStatus.Rejected)
        {
            TempData["Toast"] = "⚠️ Cannot assign to IT: Manager approval required and ticket must not be rejected.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Update Security approval
        serviceRequest.SecurityApprovalStatus = ApprovalStatus.Approved;
        serviceRequest.SecurityApprovalDate = DateTime.UtcNow;
        serviceRequest.SecurityApprovalName = currentUser.FullName;

        // Find IT user (Yazan)
        var itUser = await _userManager.FindByEmailAsync("yazan.it@yub.com.sa");
        
        if (itUser == null)
        {
            _logger.LogError("IT user (yazan.it@yub.com.sa) not found. Cannot assign ticket {TicketId} to IT.", ticket.Id);
            TempData["Toast"] = "⚠️ IT user not found. Please contact administrator.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }
        
        // CRITICAL: Assign to IT ONLY if all conditions are met
        // ManagerApprovalStatus == Approved AND SecurityApprovalStatus == Approved (just set above)
        ticket.AssignedToId = itUser.Id;
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
            Action = "Security Approved Service Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Load for notification
        await _context.Entry(serviceRequest).Reference(sr => sr.Ticket).LoadAsync();
        await _notificationService.NotifyServiceRequestITAsync(serviceRequest);

        TempData["Toast"] = "✅ Security approval granted. Ticket assigned to IT for execution.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectSecurityService(int id, [FromForm] string? comment)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.SelectedManager)
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        if (serviceRequest is null)
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

        // SECURITY CAN REVIEW TICKETS ASSIGNED TO THEM
        // This is required for the Security approval workflow stage
        // Security must be able to review tickets assigned to them after Manager approval
        // Only allow if ticket is assigned to current Security user (for Security approval stage)
        if (ticket.AssignedToId != currentUser.Id)
        {
            return Forbid();
        }

        // Verify manager has approved
        if (serviceRequest.ManagerApprovalStatus != ApprovalStatus.Approved)
        {
            TempData["Toast"] = "⚠️ Manager approval is required before Security review.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Verify the request is still pending Security approval
        if (serviceRequest.SecurityApprovalStatus != ApprovalStatus.Pending)
        {
            TempData["Toast"] = "⚠️ This request has already been processed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Validate comment is required for rejection
        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["Toast"] = "⚠️ A rejection reason is required.";
            return RedirectToAction(nameof(ApproveSecurityService), new { id = ticket.Id });
        }

        // Update Security approval to Rejected
        serviceRequest.SecurityApprovalStatus = ApprovalStatus.Rejected;
        serviceRequest.SecurityApprovalDate = DateTime.UtcNow;
        serviceRequest.SecurityApprovalName = currentUser.FullName;

        // Close the ticket - final rejection
        ticket.Status = TicketStatus.Rejected;
        ticket.AssignedToId = null; // Unassign - ticket is closed

        // Add rejection log
        var logNotes = $"Security rejection by {currentUser.FullName}. Reason: {comment}";

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "Security Rejected Service Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Send rejection notifications
        var ticketNumber = $"HD-{ticket.Id:D6}";
        var detailsUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, Request.Scheme);
        var subject = $"[IT Help Desk] Service Request Rejected by Security - {ticketNumber}";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Service Request Rejected</h2>
        </div>
        <div class=""content"">
            <p>Dear recipient,</p>
            <p>The Service Request {ticketNumber} has been rejected by Security Department.</p>
            <p><strong>Employee:</strong> {serviceRequest.EmployeeName}</p>
            <p><strong>Department:</strong> {serviceRequest.Department}</p>
            <p><strong>Rejected by:</strong> {currentUser.FullName}</p>
            <p><strong>Reason:</strong> {comment}</p>
            <p><a href=""{detailsUrl}"" class=""button"">View Request Details</a></p>
        </div>
    </div>
</body>
</html>";

        // Send notification to employee (ticket creator)
        if (ticket.CreatedBy != null && !string.IsNullOrWhiteSpace(ticket.CreatedBy.Email))
        {
            await _emailSender.SendEmailAsync(ticket.CreatedBy.Email, subject, body);
            _logger.LogInformation("Rejection notification sent to employee {Email} for Service Request {TicketId}", ticket.CreatedBy.Email, ticket.Id);
        }

        // Send notification to manager
        if (serviceRequest.SelectedManager != null && !string.IsNullOrWhiteSpace(serviceRequest.SelectedManager.Email))
        {
            await _emailSender.SendEmailAsync(serviceRequest.SelectedManager.Email, subject, body);
            _logger.LogInformation("Rejection notification sent to manager {Email} for Service Request {TicketId}", serviceRequest.SelectedManager.Email, ticket.Id);
        }

        TempData["Toast"] = "⚠️ Service request rejected. The ticket has been closed and notifications have been sent to employee and manager.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ExecuteServiceRequest(int id)
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

        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.SelectedManager)
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        if (serviceRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Only allow IT role to view
        var isIT = User.IsInRole("IT");
        
        if (!isIT)
        {
            return Forbid();
        }

        // IsAuthorizedIT: true if user is IT
        var isAuthorizedIT = isIT;
        
        // IsReadOnly: true if status is not InProgress (already executed/closed)
        var isReadOnly = ticket.Status != TicketStatus.InProgress;
        
        // CanExecute: true if authorized AND status is InProgress AND Security has approved
        var canExecute = isAuthorizedIT && 
                        ticket.Status == TicketStatus.InProgress &&
                        serviceRequest.SecurityApprovalStatus == ApprovalStatus.Approved;

        var viewModel = new ServiceRequestExecutionViewModel
        {
            TicketId = ticket.Id,
            Ticket = ticket,
            ServiceRequest = serviceRequest,
            IsAuthorizedIT = isAuthorizedIT,
            IsReadOnly = isReadOnly,
            CanExecute = canExecute,
            Logs = ticket.Logs.OrderByDescending(l => l.Timestamp)
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExecuteServiceRequest(int id, [FromForm] string? executionNotes, [FromForm] List<IFormFile>? attachments)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        if (serviceRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Verify the current user is IT (Yazan)
        if (!User.IsInRole("IT"))
        {
            return Forbid();
        }

        // Verify Security has approved
        if (serviceRequest.SecurityApprovalStatus != ApprovalStatus.Approved)
        {
            TempData["Toast"] = "⚠️ Security approval is required before execution.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Verify the request is still pending execution
        if (ticket.Status != TicketStatus.InProgress)
        {
            TempData["Toast"] = "⚠️ This request has already been processed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        if (string.IsNullOrWhiteSpace(executionNotes))
        {
            TempData["Toast"] = "⚠️ Execution notes are required.";
            return RedirectToAction(nameof(ExecuteServiceRequest), new { id = ticket.Id });
        }

        // Update IT approval and ticket status
        serviceRequest.ITApprovalStatus = ApprovalStatus.Approved;
        serviceRequest.ITApprovalDate = DateTime.UtcNow;
        serviceRequest.ITApprovalName = currentUser.FullName;

        ticket.Status = TicketStatus.Resolved;

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
                    _logger.LogWarning(ex, "Failed to upload attachment for ticket {TicketId} during IT execution.", ticket.Id);
                }
            }
        }

        // Add execution log
        var logNotes = $"Service request completed by {currentUser.FullName}.";
        if (!string.IsNullOrWhiteSpace(executionNotes))
        {
            logNotes += $" Notes: {executionNotes}";
        }

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "IT Completed Service Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Load for notification
        await _context.Entry(serviceRequest).Reference(sr => sr.Ticket).LoadAsync();
        await _context.Entry(serviceRequest).Reference(sr => sr.SelectedManager).LoadAsync();
        await _notificationService.NotifyServiceRequestCompletedAsync(serviceRequest);

        TempData["Toast"] = "✅ Service request completed successfully.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseServiceRequest(int id, [FromForm] string? closureReason, [FromForm] List<IFormFile>? attachments)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.TicketId == ticket.Id);

        if (serviceRequest is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Verify the current user is IT (Yazan)
        if (!User.IsInRole("IT"))
        {
            return Forbid();
        }

        // Verify Security has approved
        if (serviceRequest.SecurityApprovalStatus != ApprovalStatus.Approved)
        {
            TempData["Toast"] = "⚠️ Security approval is required before closure.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        // Verify the request is still pending execution
        if (ticket.Status != TicketStatus.InProgress)
        {
            TempData["Toast"] = "⚠️ This request has already been processed.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        if (string.IsNullOrWhiteSpace(closureReason))
        {
            TempData["Toast"] = "⚠️ Closure reason is required.";
            return RedirectToAction(nameof(ExecuteServiceRequest), new { id = ticket.Id });
        }

        // Update IT approval status (rejected) and ticket status
        serviceRequest.ITApprovalStatus = ApprovalStatus.Rejected;
        serviceRequest.ITApprovalDate = DateTime.UtcNow;
        serviceRequest.ITApprovalName = currentUser.FullName;

        ticket.Status = TicketStatus.Rejected;

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
                    _logger.LogWarning(ex, "Failed to upload attachment for ticket {TicketId} during IT closure.", ticket.Id);
                }
            }
        }

        // Add closure log
        var logNotes = $"Service request closed by {currentUser.FullName}.";
        if (!string.IsNullOrWhiteSpace(closureReason))
        {
            logNotes += $" Reason: {closureReason}";
        }

        var log = new TicketLog
        {
            TicketId = ticket.Id,
            Action = "IT Closed Service Request",
            PerformedById = currentUser.Id,
            Timestamp = DateTime.UtcNow,
            Notes = logNotes
        };

        _context.TicketLogs.Add(log);
        await _context.SaveChangesAsync();

        // Load for notification
        await _context.Entry(serviceRequest).Reference(sr => sr.Ticket).LoadAsync();
        await _context.Entry(serviceRequest).Reference(sr => sr.SelectedManager).LoadAsync();
        await _notificationService.NotifyServiceRequestCompletedAsync(serviceRequest);

        TempData["Toast"] = "⚠️ Service request has been closed.";
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
