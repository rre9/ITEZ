using System;
using System.Linq;
using System.Threading.Tasks;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ITHelpDesk.Seed;

public static class IdentitySeeder
{
    private static readonly string[] DefaultRoles = { "Admin", "Support", "Employee" };
    private const string AdminEmail = "yazan@yub.com.sa";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("IdentitySeeder");

        foreach (var roleName in DefaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
                }
            }
        }

        var adminPassword = configuration["Seed:AdminDefaultPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            logger?.LogWarning("Admin default password not configured. Skipping admin user seeding.");
            return;
        }

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FullName = "IT Help Desk Administrator",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }

            logger?.LogInformation("Seeded administrator account '{Email}'.", AdminEmail);
        }
        else
        {
            // إعادة تعيين كلمة المرور إذا كان الحساب موجوداً
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var resetResult = await userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);
            if (resetResult.Succeeded)
            {
                logger?.LogInformation("Admin password reset for existing account '{Email}'.", AdminEmail);
            }
            else
            {
                var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                logger?.LogWarning("Failed to reset admin password: {Errors}", errors);
            }

            // تأكد من تأكيد الإيميل
            if (!adminUser.EmailConfirmed)
            {
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);
            }
        }

        foreach (var role in new[] { "Admin", "Support" })
        {
            if (!await userManager.IsInRoleAsync(adminUser, role))
            {
                await userManager.AddToRoleAsync(adminUser, role);
            }
        }
    }
}

