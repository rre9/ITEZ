using System;
using System.Linq;
using System.Threading.Tasks;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ITHelpDesk.Seed;

public static class IdentitySeeder
{
    private static readonly string[] Roles = { "Employee", "Manager", "Security", "IT" };
    private const string DefaultPassword = "Test@123"; // Temporary password for development only

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Create roles if they do not exist
        foreach (var roleName in Roles)
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

        // Seed users
        var usersToSeed = new[]
        {
            new { FullName = "Mohammed Cyber", Email = "mohammed.cyber@yub.com.sa", Role = "Security" },
            new { FullName = "Yazan IT", Email = "yazan.it@yub.com.sa", Role = "IT" },
            new { FullName = "Mashael IT R", Email = "mashael.itr@yub.com.sa", Role = "Manager" },
            new { FullName = "Abeer Finance", Email = "abeer.finance@yub.com.sa", Role = "Manager" },
            new { FullName = "Mashael Aggregator", Email = "mashael.agg@yub.com.sa", Role = "Manager" }
        };

        foreach (var userInfo in usersToSeed)
        {
            var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
            
            if (existingUser == null)
            {
                // Create user only if they do not already exist
                var user = new ApplicationUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    FullName = userInfo.FullName,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, DefaultPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create user '{userInfo.Email}': {errors}");
                }

                // Add user to role
                var addToRoleResult = await userManager.AddToRoleAsync(user, userInfo.Role);
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to add user '{userInfo.Email}' to role '{userInfo.Role}': {errors}");
                }

                Console.WriteLine($"User created: {userInfo.FullName} ({userInfo.Email}) - Role: {userInfo.Role}");
            }
            else
            {
                // User exists - reset password to default and ensure role is correct
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(existingUser);
                var resetResult = await userManager.ResetPasswordAsync(existingUser, resetToken, DefaultPassword);
                
                if (resetResult.Succeeded)
                {
                    Console.WriteLine($"Password reset for existing user: {userInfo.FullName} ({userInfo.Email})");
                }
                else
                {
                    var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                    Console.WriteLine($"Warning: Failed to reset password for '{userInfo.Email}': {errors}");
                }

                // Ensure user has the correct role
                var userRoles = await userManager.GetRolesAsync(existingUser);
                if (!userRoles.Contains(userInfo.Role))
                {
                    // Remove from other roles and add to correct role
                    if (userRoles.Any())
                    {
                        await userManager.RemoveFromRolesAsync(existingUser, userRoles);
                    }
                    var addToRoleResult = await userManager.AddToRoleAsync(existingUser, userInfo.Role);
                    if (addToRoleResult.Succeeded)
                    {
                        Console.WriteLine($"Role updated for {userInfo.FullName} ({userInfo.Email}) to {userInfo.Role}");
                    }
                }

                // Update FullName if it's different
                if (existingUser.FullName != userInfo.FullName)
                {
                    existingUser.FullName = userInfo.FullName;
                    await userManager.UpdateAsync(existingUser);
                    Console.WriteLine($"FullName updated for {userInfo.Email}");
                }

                // Ensure EmailConfirmed is true
                if (!existingUser.EmailConfirmed)
                {
                    existingUser.EmailConfirmed = true;
                    await userManager.UpdateAsync(existingUser);
                }

                Console.WriteLine($"User already exists: {userInfo.FullName} ({userInfo.Email}) - Role: {userInfo.Role}");
            }
        }
    }
}

