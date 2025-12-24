using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;

namespace ITHelpDesk.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _environment;

    public ForgotPasswordModel(
        UserManager<ApplicationUser> userManager, 
        IEmailSender emailSender,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _environment = environment;
    }

    [TempData]
    public string? Toast { get; set; }

    [TempData]
    public string? ResetLink { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                Toast = "✅ If that email is registered, a reset link will be sent shortly.";
                return RedirectToPage();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code = encodedCode, email = user.Email },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Reset your password",
                $"Reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            // في Development mode، اعرض الرابط مباشرة
            if (_environment.IsDevelopment())
            {
                ResetLink = callbackUrl;
                Toast = $"✅ Password reset link generated. <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}' target='_blank' class='text-decoration-underline'>Click here to reset your password</a>";
            }
            else
            {
                Toast = "✅ Password reset instructions have been sent if the email exists in our system.";
            }
            
            return RedirectToPage();
        }

        return Page();
    }
}
