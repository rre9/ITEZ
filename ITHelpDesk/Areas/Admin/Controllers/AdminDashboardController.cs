using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Areas.Admin.Controllers;

/// <summary>
/// Admin Dashboard Controller for the new Admin Panel.
/// This controller is completely separate from the legacy Admin role and AdminController.
/// Access is restricted to UserAdmin role only.
/// 
/// ⚠️ CRITICAL GUARDRAILS:
/// - UserAdmin role MUST NEVER be used outside this Admin Panel area
/// - This controller MUST NOT access workflow, ticket, or approval logic
/// - This controller MUST NOT modify legacy Admin role or AdminController
/// - All functionality is limited to user management ONLY
/// </summary>
[Area("Admin")]
[Authorize(Roles = "UserAdmin")]
public class AdminDashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminDashboardController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// Main dashboard view showing user statistics (read-only).
    /// This dashboard does NOT display workflow, ticket, or approval data.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var allUsers = await _userManager.Users
            .AsNoTracking()
            .ToListAsync();

        var usersWithRoles = new List<UserInfoViewModel>();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserInfoViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "No Role",
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed
            });
        }

        var viewModel = new AdminDashboardViewModel
        {
            TotalUsers = allUsers.Count,
            ActiveUsers = allUsers.Count(u => u.IsActive),
            DisabledUsers = allUsers.Count(u => !u.IsActive),
            Users = usersWithRoles.OrderBy(u => u.FullName).ToList()
        };

        return View(viewModel);
    }
}

/// <summary>
/// View model for user information display.
/// </summary>
public class UserInfoViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
}

/// <summary>
/// View model for the Admin Dashboard.
/// </summary>
public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int DisabledUsers { get; set; }
    public List<UserInfoViewModel> Users { get; set; } = new();
}

