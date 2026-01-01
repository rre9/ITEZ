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

namespace ITHelpDesk.Controllers
{
    [Authorize]
    public class SystemChangeRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SystemChangeRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "IT,Security,Admin")]
        public IActionResult Create()
        {
            var vm = new SystemChangeRequestCreateViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "IT,Security,Admin")]
        public async Task<IActionResult> Create(SystemChangeRequestCreateViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Challenge();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Create a ticket and embed the change request details into the description
            var title = $"System Change Request: {model.Title}";
            var description = $"Requester: {model.RequesterName}\nDepartment: IT\nPhone: {model.PhoneNumber}\n\nChange Description:\n{model.ChangeDescription}\n\nChange Reason:\n{model.ChangeReason}\n\nImpact:\nType={model.ChangeType}, Priority={model.ChangePriority}, Impact={model.ChangeImpact}\n\nAffected Assets:\n{model.AffectedAssets}\n\nImplementation Plan:\n{model.ImplementationPlan}\n\nBackout Plan:\n{model.BackoutPlan}\n\nImplementer: {model.ImplementerName}\nExecution Date: {model.ExecutionDate?.ToString("yyyy-MM-dd")}";

            // Assign directly to Security (Mohammed Cyber) for initial review
            var securityUser = await _userManager.FindByEmailAsync("mohammed.cyber@yub.com.sa");

            // Determine department: if creator is IT, set to Security, otherwise IT
            var isCreatorIT = await _userManager.IsInRoleAsync(currentUser, "IT");
            var department = isCreatorIT ? "Security" : "IT";

            var ticket = new Ticket
            {
                Title = title,
                Description = description,
                Department = department,
                Priority = TicketPriority.Medium,
                Status = TicketStatus.New,
                CreatedById = currentUser.Id,
                AssignedToId = securityUser?.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Log creation
            _context.TicketLogs.Add(new TicketLog
            {
                TicketId = ticket.Id,
                Action = "System Change Request Created",
                PerformedById = currentUser.Id,
                Timestamp = DateTime.UtcNow,
                Notes = $"System change request created by {currentUser.FullName}. Assigned to Security for review."
            });
            await _context.SaveChangesAsync();

            TempData["Toast"] = "✅ System change request submitted successfully.";
            return RedirectToAction("MyTickets", "Tickets");
        }

        [HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Attachments)
                .Include(t => t.Logs)
                .ThenInclude(l => l.PerformedBy)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket is null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Challenge();
            if (ticket.AssignedToId != currentUser.Id && !User.IsInRole("Admin")) return Forbid();

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveConfirmed(int id, string? comment)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket is null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Challenge();
            if (ticket.AssignedToId != currentUser.Id && !User.IsInRole("Admin")) return Forbid();

            // Forward back to Security (Mohammed Cyber) after manager approval
            var securityUser = await _userManager.FindByEmailAsync("mohammed.cyber@yub.com.sa");
            if (securityUser != null)
            {
                ticket.AssignedToId = securityUser.Id;
            }

            // Mark in description that manager has approved
            if (!ticket.Description.Contains("ManagerApprovalStatus=Approved"))
            {
                ticket.Description = "ManagerApprovalStatus=Approved\n" + ticket.Description;
            }

            ticket.Status = TicketStatus.New;
            _context.TicketLogs.Add(new TicketLog
            {
                TicketId = ticket.Id,
                Action = "Manager Approved (System Change)",
                PerformedById = currentUser.Id,
                Timestamp = DateTime.UtcNow,
                Notes = $"Manager {currentUser.FullName} approved and forwarded back to Security for final review." + (string.IsNullOrWhiteSpace(comment) ? string.Empty : " Comment: " + comment)
            });

            await _context.SaveChangesAsync();
            TempData["Toast"] = "✅ Approved and forwarded back to Security.";
            return RedirectToAction("TeamRequests", "Tickets");
        }

        [HttpGet]
        public async Task<IActionResult> ApproveSecurity(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Attachments)
                .Include(t => t.Logs)
                .ThenInclude(l => l.PerformedBy)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket is null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Challenge();
            if (!User.IsInRole("Security") && !User.IsInRole("Admin")) return Forbid();

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSecurityConfirmed(int id, string? comment)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket is null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Challenge();
            if (!User.IsInRole("Security") && !User.IsInRole("Admin")) return Forbid();

            // Assign to Yazan IT specifically
            var itUser = await _userManager.FindByEmailAsync("yazan.it@yub.com.sa");
            if (itUser != null)
            {
                ticket.AssignedToId = itUser.Id;
            }

            ticket.Status = TicketStatus.InProgress;
            _context.TicketLogs.Add(new TicketLog
            {
                TicketId = ticket.Id,
                Action = "Security Approved (System Change)",
                PerformedById = currentUser.Id,
                Timestamp = DateTime.UtcNow,
                Notes = $"Security {currentUser.FullName} approved and forwarded to IT." + (string.IsNullOrWhiteSpace(comment) ? string.Empty : " Comment: " + comment)
            });

            await _context.SaveChangesAsync();
            TempData["Toast"] = "✅ Security approved. Assigned to IT.";
            return RedirectToAction("TeamRequests", "Tickets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForwardToManager(int id, string? comment)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket is null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Challenge();
            if (!User.IsInRole("Security") && !User.IsInRole("Admin")) return Forbid();

            // Forward to Abeer Finance (Manager) for review
            var managerUser = await _userManager.FindByEmailAsync("mashael.agg@yub.com.sa");

            if (managerUser != null)
            {
                ticket.AssignedToId = managerUser.Id;
            }

            var notes = $"Security forwarded to manager {managerUser?.FullName ?? "Abeer Finance"} for review.";
            if (!string.IsNullOrWhiteSpace(comment))
            {
                notes += $" Comment: {comment}";
            }

            _context.TicketLogs.Add(new TicketLog
            {
                TicketId = ticket.Id,
                Action = "Forwarded to Manager",
                PerformedById = currentUser.Id,
                Timestamp = DateTime.UtcNow,
                Notes = notes
            });

            await _context.SaveChangesAsync();
            TempData["Toast"] = "🔁 Forwarded to manager for review.";
            return RedirectToAction("TeamRequests", "Tickets");
        }

        /// <summary>
        /// Helper: Extract SelectedManagerId from the first line of Description (format: SelectedManagerId={id})
        /// </summary>
        private static string? ExtractOriginalManagerId(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            var lines = description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("SelectedManagerId=", StringComparison.OrdinalIgnoreCase))
                {
                    var id = line.Substring("SelectedManagerId=".Length).Trim();
                    return string.IsNullOrWhiteSpace(id) ? null : id;
                }
            }

            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket is null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Challenge();

            ticket.Status = TicketStatus.Rejected;
            _context.TicketLogs.Add(new TicketLog
            {
                TicketId = ticket.Id,
                Action = "Rejected (System Change)",
                PerformedById = currentUser.Id,
                Timestamp = DateTime.UtcNow,
                Notes = $"Rejected by {currentUser.FullName}."
            });

            await _context.SaveChangesAsync();
            TempData["Toast"] = "⛔ Request rejected.";
            return RedirectToAction("TeamRequests", "Tickets");
        }

        [HttpGet]
        public async Task<IActionResult> Execute(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Attachments)
                .Include(t => t.Logs)
                .ThenInclude(l => l.PerformedBy)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket is null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Challenge();
            if (!User.IsInRole("IT") && !User.IsInRole("Admin")) return Forbid();
            if (!User.IsInRole("Admin") && ticket.AssignedToId != currentUser.Id) return Forbid();

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Execute(int id, string? executionNotes)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket is null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null) return Challenge();
            if (!User.IsInRole("IT") && !User.IsInRole("Admin")) return Forbid();
            if (!User.IsInRole("Admin") && ticket.AssignedToId != currentUser.Id) return Forbid();

            ticket.Status = TicketStatus.Resolved;
            _context.TicketLogs.Add(new TicketLog
            {
                TicketId = ticket.Id,
                Action = "Executed (System Change)",
                PerformedById = currentUser.Id,
                Timestamp = DateTime.UtcNow,
                Notes = $"System change executed by {currentUser.FullName}." + (string.IsNullOrWhiteSpace(executionNotes) ? string.Empty : " Notes: " + executionNotes)
            });

            await _context.SaveChangesAsync();
            TempData["Toast"] = "✅ Change executed and ticket resolved.";
            return RedirectToAction("MyTasks", "Tickets");
        }
    }
}
