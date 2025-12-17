using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;

namespace ITHelpDesk.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AdminController> _logger;
    private readonly ApplicationDbContext _context;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailSender emailSender,
        ILogger<AdminController> logger,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? role)
    {
        var roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync();

        var roleOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("All roles", string.Empty)
        };
        roleOptions.AddRange(roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(r, r, string.Equals(r, role, StringComparison.OrdinalIgnoreCase))));

        var usersQuery = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            usersQuery = usersQuery.Where(u =>
                u.FullName.Contains(term) ||
                (u.Email != null && u.Email.Contains(term)));
        }

        var users = await usersQuery
            .OrderBy(u => u.FullName)
            .Take(200)
            .ToListAsync();

        var result = new List<AdminUserViewModel>();

        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            if (!string.IsNullOrWhiteSpace(role) && !userRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            result.Add(new AdminUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = userRoles.OrderBy(r => r).ToList(),
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow,
                LockoutEnd = user.LockoutEnd,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = null
            });
        }

        var viewModel = new AdminUsersViewModel
        {
            Users = result,
            Search = search,
            Role = role,
            RoleOptions = roleOptions
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        TempData["Toast"] = $"✅ {user.FullName} is now an Admin.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
        {
            TempData["Toast"] = "⚠️ You cannot remove your own Admin role.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        TempData["Toast"] = $"✅ {user.FullName} is no longer an Admin.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeSupport(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!await _userManager.IsInRoleAsync(user, "Support"))
        {
            await _userManager.AddToRoleAsync(user, "Support");
        }

        TempData["Toast"] = $"✅ {user.FullName} added to Support.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSupport(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (await _userManager.IsInRoleAsync(user, "Support"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Support");
        }

        TempData["Toast"] = $"✅ {user.FullName} removed from Support.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

        TempData["Toast"] = $"✅ {user.FullName} account locked.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);

        TempData["Toast"] = $"✅ {user.FullName} account unlocked.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            TempData["Toast"] = "⚠️ User does not have an email on file.";
            return RedirectToAction(nameof(Index));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = Url.Action(
            "ResetPassword",
            "Account",
            new { area = "Identity", userId = user.Id, code = encodedToken },
            Request.Scheme)!;

        var body = $@"
<p>Hello {user.FullName},</p>
<p>An administrator has requested a password reset for your IT Help Desk account.</p>
<p>Please reset your password by <a href=\""{callbackUrl}\"">clicking here</a>.</p>
<p>If you did not request this change, please contact support immediately.</p>
<p>&mdash; IT Help Desk Team</p>";

        await _emailSender.SendEmailAsync(user.Email, "[IT Help Desk] Password reset requested", body);

        TempData["Toast"] = $"✅ Password reset email sent to {user.Email}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        // منع حذف نفسك
        var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
        {
            TempData["Toast"] = "⚠️ You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // حذف السجلات المرتبطة أولاً
            var logsToDelete = await _context.TicketLogs
                .Where(l => l.PerformedById == user.Id)
                .ToListAsync();
            
            if (logsToDelete.Any())
            {
                _context.TicketLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} ticket logs associated with user {UserId}.", logsToDelete.Count, user.Id);
            }

            // حذف التذاكر التي أنشأها المستخدم (إذا كان لديه تذاكر)
            var ticketsCreatedByUser = await _context.Tickets
                .Where(t => t.CreatedById == user.Id)
                .ToListAsync();
            
            if (ticketsCreatedByUser.Any())
            {
                // حذف المرفقات المرتبطة
                var ticketIds = ticketsCreatedByUser.Select(t => t.Id).ToList();
                var attachmentsToDelete = await _context.TicketAttachments
                    .Where(a => ticketIds.Contains(a.TicketId))
                    .ToListAsync();
                
                if (attachmentsToDelete.Any())
                {
                    _context.TicketAttachments.RemoveRange(attachmentsToDelete);
                }

                // حذف السجلات المرتبطة بالتذاكر
                var logsToDeleteForTickets = await _context.TicketLogs
                    .Where(l => ticketIds.Contains(l.TicketId))
                    .ToListAsync();
                
                if (logsToDeleteForTickets.Any())
                {
                    _context.TicketLogs.RemoveRange(logsToDeleteForTickets);
                }

                // حذف التذاكر
                _context.Tickets.RemoveRange(ticketsCreatedByUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} tickets created by user {UserId}.", ticketsCreatedByUser.Count, user.Id);
            }

            // تحديث التذاكر المعينة للمستخدم (إزالة التعيين)
            var ticketsAssignedToUser = await _context.Tickets
                .Where(t => t.AssignedToId == user.Id)
                .ToListAsync();
            
            if (ticketsAssignedToUser.Any())
            {
                foreach (var ticket in ticketsAssignedToUser)
                {
                    ticket.AssignedToId = null;
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Unassigned {Count} tickets from user {UserId}.", ticketsAssignedToUser.Count, user.Id);
            }

            // الآن يمكن حذف المستخدم بأمان
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} ({Email}) deleted successfully.", user.Id, user.Email);
                TempData["Toast"] = $"✅ User {user.FullName} ({user.Email}) has been deleted.";
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to delete user {UserId}: {Errors}", user.Id, errors);
                TempData["Toast"] = $"⚠️ Failed to delete user: {errors}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}.", user.Id);
            TempData["Toast"] = $"⚠️ An error occurred while deleting the user. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAllTickets()
    {
        try
        {
            // Delete all ticket logs (cascade will handle this, but being explicit)
            var logsCount = await _context.TicketLogs.CountAsync();
            _context.TicketLogs.RemoveRange(_context.TicketLogs);
            
            // Delete all ticket attachments
            var attachmentsCount = await _context.TicketAttachments.CountAsync();
            _context.TicketAttachments.RemoveRange(_context.TicketAttachments);
            
            // Delete all tickets (this will cascade delete logs and attachments)
            var ticketsCount = await _context.Tickets.CountAsync();
            _context.Tickets.RemoveRange(_context.Tickets);
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted {TicketsCount} tickets, {AttachmentsCount} attachments, and {LogsCount} logs.", 
                ticketsCount, attachmentsCount, logsCount);
            
            TempData["Toast"] = $"✅ All tickets deleted successfully. ({ticketsCount} tickets, {attachmentsCount} attachments, {logsCount} logs)";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete all tickets.");
            TempData["Toast"] = "⚠️ Failed to delete tickets. Please try again.";
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["Toast"] = "⚠️ Password cannot be empty.";
            return RedirectToAction(nameof(Index));
        }

        if (newPassword.Length < 6)
        {
            TempData["Toast"] = "⚠️ Password must be at least 6 characters long.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        // Remove old password
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
        if (!removePasswordResult.Succeeded)
        {
            var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
            TempData["Toast"] = $"⚠️ Failed to change password: {errors}";
            return RedirectToAction(nameof(Index));
        }

        // Add new password
        var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
        if (addPasswordResult.Succeeded)
        {
            _logger.LogInformation("Admin {AdminId} changed password for user {UserId} ({Email}).", 
                User.FindFirstValue(ClaimTypes.NameIdentifier), user.Id, user.Email);
            TempData["Toast"] = $"✅ Password changed successfully for {user.FullName}.";
        }
        else
        {
            var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
            TempData["Toast"] = $"⚠️ Failed to set new password: {errors}";
        }

        return RedirectToAction(nameof(Index));
    }
}

