using System;
using System.Linq;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ITHelpDesk.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

            db.Database.EnsureCreated();

            SeedRoles(roleManager).GetAwaiter().GetResult();
            SeedUsers(userManager).GetAwaiter().GetResult();
        });
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "Support", "Employee" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsers(UserManager<ApplicationUser> userManager)
    {
        async Task<ApplicationUser> EnsureUserAsync(string email, string fullName, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    FullName = fullName
                };
                await userManager.CreateAsync(user, password);
            }
            return user;
        }

        var admin = await EnsureUserAsync("admin@yub.com.sa", "Admin User", "Admin#12345!");
        await userManager.AddToRoleAsync(admin, "Admin");
        await userManager.AddToRoleAsync(admin, "Support");

        var support = await EnsureUserAsync("support@yub.com.sa", "Support User", "Admin#12345!");
        await userManager.AddToRoleAsync(support, "Support");

        var employee = await EnsureUserAsync("employee@yub.com.sa", "Employee User", "Admin#12345!");
        await userManager.AddToRoleAsync(employee, "Employee");
    }
}
