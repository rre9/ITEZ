using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.Seed;
using ITHelpDesk.Services;
using ITHelpDesk.Services.Abstractions;
using ITHelpDesk.Services.Authorization;
using ITHelpDesk.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to enforce TLS 1.2 and TLS 1.3 only
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        // Enforce TLS 1.2 and TLS 1.3 only (disable older versions)
        httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    });
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

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
builder.Services.Configure<DepartmentOptions>(builder.Configuration.GetSection("Departments"));
builder.Services.AddScoped<IDepartmentProvider, DepartmentProvider>();
builder.Services.AddScoped<ITicketQueryService, TicketQueryService>();
builder.Services.AddScoped<IAuthorizationHandler, TicketAccessHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITHelpDesk.Services.Notifications.INotificationService, ITHelpDesk.Services.Notifications.EmailNotificationService>();
builder.Services.AddScoped<ITHelpDesk.Services.Email.IEmailService, ITHelpDesk.Services.Email.MockEmailService>();

// Data Protection API for encrypting sensitive data
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")))
    .SetApplicationName("ITHelpDesk")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Rate Limiting to prevent brute force attacks
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Login endpoint - stricter limits
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5, // 5 attempts per window
                Window = TimeSpan.FromMinutes(15) // 15 minute window
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    
    // Auto-create Assets tables if they don't exist
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.ExecuteSqlRawAsync(@"
        -- إنشاء جدول المنتجات (Products)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
        BEGIN
            CREATE TABLE Products (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                ProductType NVARCHAR(100) NOT NULL,
                ProductName NVARCHAR(150) NOT NULL,
                Manufacturer NVARCHAR(100) NOT NULL,
                PartNo NVARCHAR(50) NULL,
                Cost DECIMAL(18, 2) NOT NULL DEFAULT 0,
                Description NVARCHAR(500) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
            );
        END

        -- إنشاء جدول الموردين (Vendors)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vendors')
        BEGIN
            CREATE TABLE Vendors (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                VendorName NVARCHAR(150) NOT NULL,
                Currency NVARCHAR(5) NOT NULL DEFAULT 'SR',
                DoorNumber NVARCHAR(10) NULL,
                Landmark NVARCHAR(100) NULL,
                PostalCode NVARCHAR(10) NULL,
                Country NVARCHAR(50) NULL,
                Fax NVARCHAR(20) NULL,
                FirstName NVARCHAR(50) NULL,
                Street NVARCHAR(100) NULL,
                City NVARCHAR(50) NULL,
                StateProvince NVARCHAR(50) NULL,
                PhoneNo NVARCHAR(20) NULL,
                Email NVARCHAR(100) NULL,
                Description NVARCHAR(500) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
            );
        END

        -- إنشاء جدول حالات الأصول (AssetStates)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AssetStates')
        BEGIN
            CREATE TABLE AssetStates (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Status INT NOT NULL DEFAULT 1,
                AssociatedTo NVARCHAR(50) NULL,
                Site NVARCHAR(50) NULL,
                StateComments NVARCHAR(500) NULL,
                UserId NVARCHAR(450) NULL,
                Department NVARCHAR(100) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول تفاصيل الشبكة (NetworkDetails)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NetworkDetails')
        BEGIN
            CREATE TABLE NetworkDetails (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                IPAddress NVARCHAR(15) NULL,
                MACAddress NVARCHAR(17) NULL,
                NIC NVARCHAR(50) NULL,
                Network NVARCHAR(100) NULL,
                DefaultGateway NVARCHAR(15) NULL,
                DHCPEnabled BIT NOT NULL DEFAULT 0,
                DHCPServer NVARCHAR(15) NULL,
                DNSHostname NVARCHAR(100) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول معلومات الكمبيوتر (ComputerInfos)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ComputerInfos')
        BEGIN
            CREATE TABLE ComputerInfos (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                ServiceTag NVARCHAR(50) NULL,
                Manufacturer NVARCHAR(100) NULL,
                BiosDate DATETIME2 NULL,
                Domain NVARCHAR(100) NULL,
                SMBiosVersion NVARCHAR(50) NULL,
                BiosVersion NVARCHAR(50) NULL,
                BiosManufacturer NVARCHAR(100) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول معلومات نظام التشغيل (OperatingSystemInfos)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OperatingSystemInfos')
        BEGIN
            CREATE TABLE OperatingSystemInfos (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100) NULL,
                Version NVARCHAR(50) NULL,
                BuildNumber NVARCHAR(50) NULL,
                ServicePack NVARCHAR(50) NULL,
                ProductId NVARCHAR(100) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول تفاصيل الذاكرة (MemoryDetails)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MemoryDetails')
        BEGIN
            CREATE TABLE MemoryDetails (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                RAM INT NULL,
                VirtualMemory INT NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول المعالجات (Processors)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Processors')
        BEGIN
            CREATE TABLE Processors (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                ProcessorInfo NVARCHAR(150) NULL,
                Manufacturer NVARCHAR(100) NULL,
                ClockSpeedMHz INT NULL,
                NumberOfCores INT NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول الأقراص الصلبة (HardDisks)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HardDisks')
        BEGIN
            CREATE TABLE HardDisks (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Model NVARCHAR(100) NULL,
                SerialNumber NVARCHAR(100) NULL,
                Manufacturer NVARCHAR(100) NULL,
                CapacityGB INT NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول لوحات المفاتيح (Keyboards)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Keyboards')
        BEGIN
            CREATE TABLE Keyboards (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                KeyboardType NVARCHAR(100) NULL,
                Manufacturer NVARCHAR(100) NULL,
                SerialNumber NVARCHAR(100) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول الفأرة (Mice)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Mice')
        BEGIN
            CREATE TABLE Mice (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                MouseType NVARCHAR(100) NULL,
                SerialNumber NVARCHAR(100) NULL,
                MouseButtons INT NULL,
                Manufacturer NVARCHAR(100) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول الشاشات (Monitors)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Monitors')
        BEGIN
            CREATE TABLE Monitors (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                MonitorType NVARCHAR(100) NULL,
                SerialNumber NVARCHAR(100) NULL,
                Manufacturer NVARCHAR(100) NULL,
                MaxResolution NVARCHAR(100) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول تفاصيل الأجهزة المحمولة (MobileDetails)
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MobileDetails')
        BEGIN
            CREATE TABLE MobileDetails (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                IMEI NVARCHAR(20) NULL,
                Model NVARCHAR(100) NULL,
                ModelNo NVARCHAR(100) NULL,
                TotalCapacityGB INT NULL,
                AvailableCapacityGB INT NULL,
                ModemFirmwareVersion NVARCHAR(100) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL
            );
        END

        -- إنشاء جدول الأصول الرئيسي (Assets) - TPH Table
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assets')
        BEGIN
            CREATE TABLE Assets (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                AssetType NVARCHAR(100) NOT NULL,
                Name NVARCHAR(150) NOT NULL,
                ProductId INT NOT NULL,
                SerialNumber NVARCHAR(100) NULL,
                AssetTag NVARCHAR(50) NULL,
                VendorId INT NULL,
                PurchaseCost DECIMAL(18, 2) NOT NULL DEFAULT 0,
                ExpiryDate DATETIME2 NULL,
                Location NVARCHAR(150) NULL,
                AcquisitionDate DATETIME2 NULL,
                WarrantyExpiryDate DATETIME2 NULL,
                AssetStateId INT NULL,
                NetworkDetailsId INT NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 NULL,
                CreatedById NVARCHAR(450) NULL,
                -- Workstation/Computer/Server/VirtualHost columns
                ComputerInfoId INT NULL,
                OperatingSystemInfoId INT NULL,
                MemoryDetailsId INT NULL,
                ProcessorId INT NULL,
                HardDiskId INT NULL,
                KeyboardId INT NULL,
                MouseId INT NULL,
                MonitorId INT NULL,
                -- AccessPoint columns
                APType NVARCHAR(50) NULL,
                APDescription NVARCHAR(255) NULL,
                APLocation NVARCHAR(255) NULL,
                InstallationDate DATETIME2 NULL,
                APModel NVARCHAR(50) NULL,
                SupportedBands NVARCHAR(50) NULL,
                Channel INT NULL,
                -- MobileDevice columns
                MobileDetailsId INT NULL,
                -- VirtualHost columns
                VMPlatform NVARCHAR(100) NULL,
                StorageLocation NVARCHAR(200) NULL,
                HostName NVARCHAR(100) NULL,
                -- VirtualMachine columns
                VirtualHostId INT NULL
            );
        END
        
        -- إضافة الأعمدة إذا كان الجدول موجود لكن الأعمدة مفقودة
        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Assets')
        BEGIN
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'ComputerInfoId')
                ALTER TABLE Assets ADD ComputerInfoId INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'OperatingSystemInfoId')
                ALTER TABLE Assets ADD OperatingSystemInfoId INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'MemoryDetailsId')
                ALTER TABLE Assets ADD MemoryDetailsId INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'ProcessorId')
                ALTER TABLE Assets ADD ProcessorId INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'HardDiskId')
                ALTER TABLE Assets ADD HardDiskId INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'KeyboardId')
                ALTER TABLE Assets ADD KeyboardId INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'MouseId')
                ALTER TABLE Assets ADD MouseId INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'MonitorId')
                ALTER TABLE Assets ADD MonitorId INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'MobileDetailsId')
                ALTER TABLE Assets ADD MobileDetailsId INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'VMPlatform')
                ALTER TABLE Assets ADD VMPlatform NVARCHAR(100) NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'StorageLocation')
                ALTER TABLE Assets ADD StorageLocation NVARCHAR(200) NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'HostName')
                ALTER TABLE Assets ADD HostName NVARCHAR(100) NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'VirtualHostId')
                ALTER TABLE Assets ADD VirtualHostId INT NULL;
        END
        
        -- إضافة Foreign Keys إذا الجدول موجود لكن الـ Constraints مو موجودة
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_Products')
        BEGIN
            ALTER TABLE Assets ADD CONSTRAINT FK_Assets_Products FOREIGN KEY (ProductId) REFERENCES Products(Id);
        END
        
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_Vendors')
        BEGIN
            ALTER TABLE Assets ADD CONSTRAINT FK_Assets_Vendors FOREIGN KEY (VendorId) REFERENCES Vendors(Id);
        END
        
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetStates')
        BEGIN
            ALTER TABLE Assets ADD CONSTRAINT FK_Assets_AssetStates FOREIGN KEY (AssetStateId) REFERENCES AssetStates(Id);
        END
        
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_NetworkDetails')
        BEGIN
            ALTER TABLE Assets ADD CONSTRAINT FK_Assets_NetworkDetails FOREIGN KEY (NetworkDetailsId) REFERENCES NetworkDetails(Id);
        END
    ");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Rate Limiting must be after UseRouting
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
