using ITHelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Areas.Admin.Controllers;

/// <summary>
/// Users Management Controller for the new Admin Panel.
/// This controller handles user creation and management.
/// Access is restricted to UserAdmin role only.
/// 
/// ⚠️ CRITICAL GUARDRAILS:
/// - UserAdmin role MUST NEVER be used outside this Admin Panel area
/// - This controller MUST NOT access workflow, ticket, or approval logic
/// - This controller MUST NOT modify legacy Admin role or AdminController
/// - User creation is password-less (prepared for SAML SSO)
/// - All functionality is limited to user management ONLY
/// </summary>
[Area("Admin")]
[Authorize(Roles = "UserAdmin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Display list of all users.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var allUsers = await _userManager.Users.ToListAsync();
        var usersWithRoles = new List<UserListViewModel>();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserListViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "No Role",
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed
            });
        }

        return View(usersWithRoles.OrderBy(u => u.FullName).ToList());
    }

    /// <summary>
    /// Display form to create a new user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateUserViewModel
        {
            AvailableRoles = await GetRoleSelectListAsync()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Create a new user without password (prepared for SAML SSO).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await GetRoleSelectListAsync();
            return View(model);
        }

        // Validate email uniqueness
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
            model.AvailableRoles = await GetRoleSelectListAsync();
            return View(model);
        }

        // Validate role exists
        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
        if (!roleExists)
        {
            ModelState.AddModelError(nameof(model.Role), "The selected role does not exist.");
            model.AvailableRoles = await GetRoleSelectListAsync();
            return View(model);
        }

        try
        {
            // Create user without password (SAML SSO will handle authentication)
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true, // Auto-confirm for SAML SSO preparation
                IsActive = true
            };

            // Note: UserManager.CreateAsync requires a password, but we'll use a temporary placeholder
            // In production with SAML, password authentication will be disabled
            // For now, we generate a secure random password that will never be used
            var tempPassword = GenerateSecureRandomPassword();

            var createResult = await _userManager.CreateAsync(user, tempPassword);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                model.AvailableRoles = await GetRoleSelectListAsync();
                return View(model);
            }

            // Assign role
            var addToRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addToRoleResult.Succeeded)
            {
                // If role assignment fails, delete the user to maintain consistency
                await _userManager.DeleteAsync(user);
                foreach (var error in addToRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, $"Failed to assign role: {error.Description}");
                }
                model.AvailableRoles = await GetRoleSelectListAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = $"User '{model.FullName}' ({model.Email}) has been created successfully with role '{model.Role}'.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log error (in production, use ILogger)
            ModelState.AddModelError(string.Empty, "An error occurred while creating the user. Please try again.");
            model.AvailableRoles = await GetRoleSelectListAsync();
            return View(model);
        }
    }

    /// <summary>
    /// Get role select list for dropdown.
    /// </summary>
    private async Task<List<SelectListItem>> GetRoleSelectListAsync()
    {
        var roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync();

        return roles.Select(r => new SelectListItem
        {
            Text = r,
            Value = r
        }).ToList();
    }

    /// <summary>
    /// Generate a secure random password that will never be used (for SAML SSO preparation).
    /// This is required by UserManager.CreateAsync but will not be used for authentication.
    /// </summary>
    private string GenerateSecureRandomPassword()
    {
        // Generate a secure random password that meets Identity requirements
        // This password will never be used since SAML SSO will handle authentication
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var password = new System.Text.StringBuilder();
        
        // Ensure at least one of each required character type
        password.Append("A"); // uppercase
        password.Append("a"); // lowercase
        password.Append("1"); // digit
        password.Append("!"); // special

        // Fill the rest randomly (minimum 8 characters total)
        for (int i = password.Length; i < 16; i++)
        {
            password.Append(chars[random.Next(chars.Length)]);
        }

        // Shuffle the password
        var passwordArray = password.ToString().ToCharArray();
        for (int i = passwordArray.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
        }

        return new string(passwordArray);
    }
}

/// <summary>
/// View model for user list display.
/// </summary>
public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
}

/// <summary>
/// View model for creating a new user.
/// </summary>
public class CreateUserViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name is required.")]
    [MaxLength(150, ErrorMessage = "Full Name cannot exceed 150 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;

    public List<SelectListItem> AvailableRoles { get; set; } = new();
}

