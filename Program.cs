using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.Seed;
using ITHelpDesk.Services;
using ITHelpDesk.Validators;
using ITHelpDesk.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/AccessDenied";
});

builder.Services.AddTransient<IUserValidator<ApplicationUser>, YubEmailDomainValidator>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsSupportOrAdmin", policy =>
        policy.RequireRole("Admin", "Support"));
    options.AddPolicy("TicketAccess", policy =>
        policy.Requirements.Add(new TicketAccessRequirement()));
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<ITicketAttachmentService, TicketAttachmentService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<DevConsoleEmailSender>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.Configure<FormOptions>(options =>
{
    const long tenMegabytes = 10 * 1024 * 1024;
    options.MultipartBodyLengthLimit = tenMegabytes;
});
builder.Services.Configure<DepartmentOptions>(builder.Configuration.GetSection("Departments"));
builder.Services.AddSingleton<IDepartmentProvider, DepartmentProvider>();
builder.Services.AddScoped<ITicketQueryService, TicketQueryService>();
builder.Services.AddScoped<IAuthorizationHandler, TicketAccessHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

public partial class Program
{
}
