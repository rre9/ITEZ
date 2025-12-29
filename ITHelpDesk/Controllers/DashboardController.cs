using System;
using System.Linq;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Check if user is Mohammed (Security)
        var isMohammed = currentUser.FullName.StartsWith("Mohammed", StringComparison.OrdinalIgnoreCase) ||
                         (currentUser.Email != null && currentUser.Email.Contains("mohammed", StringComparison.OrdinalIgnoreCase));

        // Check if user is Yazan (IT)
        var isYazan = currentUser.FullName.StartsWith("Yazan", StringComparison.OrdinalIgnoreCase) ||
                      (currentUser.Email != null && currentUser.Email.Contains("yazan", StringComparison.OrdinalIgnoreCase));

        var isAdminOrSupport = User.IsInRole("Admin") || User.IsInRole("Support");
        var isEmployee = User.IsInRole("Employee");

        // Route based on role
        if (isMohammed && !isAdminOrSupport)
        {
            return RedirectToAction(nameof(Security));
        }

        if (isYazan && !isAdminOrSupport)
        {
            return RedirectToAction(nameof(IT));
        }

        // Check if user has assigned tasks (managers, security, IT all use Tasks page)
        var hasAssignedTasks = await _context.Tickets
            .AnyAsync(t => t.AssignedToId == currentUser.Id);

        if (hasAssignedTasks && !isAdminOrSupport && !isEmployee)
        {
            return RedirectToAction("MyTasks", "Tickets");
        }

        if (isAdminOrSupport)
        {
            return RedirectToAction(nameof(Admin));
        }

        if (isEmployee)
        {
            return RedirectToAction(nameof(Employee));
        }

        // Default fallback
        return RedirectToAction(nameof(Employee));
    }

    [Authorize]
    public async Task<IActionResult> Employee(string? status)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Get all access requests for tickets created by this employee
        var accessRequestQuery = _context.AccessRequests
            .Include(ar => ar.Ticket)
            .Where(ar => ar.Ticket != null && ar.Ticket.CreatedById == currentUser.Id)
            .Select(ar => ar.Ticket!)
            .Distinct();
        
        var query = accessRequestQuery.OrderByDescending(t => t.CreatedAt);

        // Load access requests with tickets
        var accessRequests = await _context.AccessRequests
            .Include(ar => ar.Ticket)
                .ThenInclude(t => t!.Logs)
            .Where(ar => ar.Ticket != null && ar.Ticket.CreatedById == currentUser.Id)
            .OrderByDescending(ar => ar.CreatedAt)
            .ToListAsync();

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            if (status == "Pending")
            {
                accessRequests = accessRequests.Where(ar => ar.ManagerApprovalStatus == ApprovalStatus.Pending ||
                                                          ar.SecurityApprovalStatus == ApprovalStatus.Pending ||
                                                          (ar.SecurityApprovalStatus == ApprovalStatus.Approved && ar.Ticket!.Status == TicketStatus.InProgress)).ToList();
            }
            else if (status == "Approved")
            {
                accessRequests = accessRequests.Where(ar => ar.SecurityApprovalStatus == ApprovalStatus.Approved &&
                                                          ar.Ticket!.Status == TicketStatus.InProgress).ToList();
            }
            else if (status == "Rejected")
            {
                accessRequests = accessRequests.Where(ar => ar.ManagerApprovalStatus == ApprovalStatus.Rejected ||
                                                          ar.SecurityApprovalStatus == ApprovalStatus.Rejected ||
                                                          ar.Ticket!.Status == TicketStatus.Rejected).ToList();
            }
            else if (status == "Completed")
            {
                accessRequests = accessRequests.Where(ar => ar.Ticket!.Status == TicketStatus.Resolved).ToList();
            }
        }

        var viewModel = new EmployeeDashboardViewModel
        {
            AccessRequests = accessRequests,
            SelectedStatus = status ?? "All"
        };

        return View(viewModel);
    }

    [Authorize]
    public async Task<IActionResult> Security()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Check if user is Mohammed
        var isMohammed = currentUser.FullName.StartsWith("Mohammed", StringComparison.OrdinalIgnoreCase) ||
                         (currentUser.Email != null && currentUser.Email.Contains("mohammed", StringComparison.OrdinalIgnoreCase));

        if (!isMohammed)
        {
            return Forbid();
        }

        // Get all access requests where manager approved (similar to TeamRequests behavior)
        // This allows Security to see all requests they've reviewed, not just pending ones
        var accessRequests = await _context.AccessRequests
            .Include(ar => ar.Ticket)
                .ThenInclude(t => t!.CreatedBy)
            .Include(ar => ar.SelectedManager)
            .Where(ar => ar.ManagerApprovalStatus == ApprovalStatus.Approved)
            .OrderByDescending(ar => ar.ManagerApprovalDate)
            .ToListAsync();

        var viewModel = new SecurityDashboardViewModel
        {
            PendingRequests = accessRequests
        };

        return View(viewModel);
    }

    [Authorize]
    public async Task<IActionResult> IT()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        // Check if user is Yazan
        var isYazan = currentUser.FullName.StartsWith("Yazan", StringComparison.OrdinalIgnoreCase) ||
                      (currentUser.Email != null && currentUser.Email.Contains("yazan", StringComparison.OrdinalIgnoreCase));

        if (!isYazan)
        {
            return Forbid();
        }

        // Get access requests where security approved and ticket is InProgress
        var accessRequests = await _context.AccessRequests
            .Include(ar => ar.Ticket)
                .ThenInclude(t => t!.CreatedBy)
            .Where(ar => ar.SecurityApprovalStatus == ApprovalStatus.Approved &&
                        ar.Ticket != null &&
                        ar.Ticket.Status == TicketStatus.InProgress)
            .OrderByDescending(ar => ar.SecurityApprovalDate)
            .ToListAsync();

        var viewModel = new ITDashboardViewModel
        {
            PendingRequests = accessRequests
        };

        return View(viewModel);
    }

    [Authorize]
    public async Task<IActionResult> Admin()
    {
        var isAdminOrSupport = User.IsInRole("Admin") || User.IsInRole("Support");
        if (!isAdminOrSupport)
        {
            return Forbid();
        }

        // Get all access requests
        var allAccessRequests = await _context.AccessRequests
            .Include(ar => ar.Ticket)
                .ThenInclude(t => t!.CreatedBy)
            .Include(ar => ar.SelectedManager)
            .OrderByDescending(ar => ar.CreatedAt)
            .ToListAsync();

        // Count pending requests
        var pendingManager = allAccessRequests.Count(ar => ar.ManagerApprovalStatus == ApprovalStatus.Pending);
        var pendingSecurity = allAccessRequests.Count(ar => ar.ManagerApprovalStatus == ApprovalStatus.Approved &&
                                                           ar.SecurityApprovalStatus == ApprovalStatus.Pending);
        var pendingIT = allAccessRequests.Count(ar => ar.SecurityApprovalStatus == ApprovalStatus.Approved &&
                                                     ar.Ticket != null &&
                                                     ar.Ticket.Status == TicketStatus.InProgress);

        var viewModel = new AdminDashboardViewModel
        {
            AllRequests = allAccessRequests,
            PendingManagerCount = pendingManager,
            PendingSecurityCount = pendingSecurity,
            PendingITCount = pendingIT
        };

        return View(viewModel);
    }
}

