using ITHelpDesk.Models;
using ITHelpDesk.Models.Assets;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Monitor = ITHelpDesk.Models.Assets.Monitor;

namespace ITHelpDesk.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<TicketLog> TicketLogs => Set<TicketLog>();
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<SystemChangeRequest> SystemChangeRequests => Set<SystemChangeRequest>();

    // Assets DbSets
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<AssetState> AssetStates => Set<AssetState>();
    public DbSet<NetworkDetails> NetworkDetails => Set<NetworkDetails>();
    public DbSet<ComputerInfo> ComputerInfos => Set<ComputerInfo>();
    public DbSet<OperatingSystemInfo> OperatingSystemInfos => Set<OperatingSystemInfo>();
    public DbSet<MemoryDetails> MemoryDetails => Set<MemoryDetails>();
    public DbSet<Processor> Processors => Set<Processor>();
    public DbSet<HardDisk> HardDisks => Set<HardDisk>();
    public DbSet<Keyboard> Keyboards => Set<Keyboard>();
    public DbSet<Mouse> Mice => Set<Mouse>();
    public DbSet<Monitor> Monitors => Set<Monitor>();
    public DbSet<MobileDetails> MobileDetails => Set<MobileDetails>();

    // Asset Types
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AccessPoint> AccessPoints => Set<AccessPoint>();
    public DbSet<Computer> Computers => Set<Computer>();
    public DbSet<Server> Servers => Set<Server>();
    public DbSet<VirtualHost> VirtualHosts => Set<VirtualHost>();
    public DbSet<VirtualMachine> VirtualMachines => Set<VirtualMachine>();
    public DbSet<Workstation> Workstations => Set<Workstation>();
    public DbSet<MobileDevice> MobileDevices => Set<MobileDevice>();
    public DbSet<Smartphone> Smartphones => Set<Smartphone>();
    public DbSet<Tablet> Tablets => Set<Tablet>();
    public DbSet<Printer> Printers => Set<Printer>();
    public DbSet<Router> Routers => Set<Router>();
    public DbSet<CiscoRouter> CiscoRouters => Set<CiscoRouter>();
    public DbSet<Switch> Switches => Set<Switch>();
    public DbSet<CiscoCatosSwitch> CiscoCatosSwitches => Set<CiscoCatosSwitch>();
    public DbSet<CiscoSwitch> CiscoSwitches => Set<CiscoSwitch>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureTicket(builder);
        ConfigureTicketAttachment(builder);
        ConfigureTicketLog(builder);
        ConfigureAccessRequest(builder);
        ConfigureServiceRequest(builder);
        ConfigureAssets(builder);
    }

    private static void ConfigureTicket(ModelBuilder builder)
    {
        builder.Entity<Ticket>(ticket =>
        {
            ticket.ToTable("Tickets");

            ticket.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            ticket.Property(t => t.Department)
                .IsRequired()
                .HasMaxLength(100);

            ticket.Property(t => t.Description)
                .IsRequired();

            ticket.Property(t => t.Priority)
                .HasConversion<string>()
                .HasMaxLength(20);

            ticket.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // CloseReason is stored as int (enum) in database, no conversion needed
            ticket.Property(t => t.CloseReason);

            ticket.Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            ticket.HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            ticket.HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            ticket.HasMany(t => t.Attachments)
                .WithOne(a => a.Ticket)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            ticket.HasMany(t => t.Logs)
                .WithOne(l => l.Ticket)
                .HasForeignKey(l => l.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTicketAttachment(ModelBuilder builder)
    {
        builder.Entity<TicketAttachment>(attachment =>
        {
            attachment.ToTable("TicketAttachments");

            attachment.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(255);

            attachment.Property(a => a.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            attachment.Property(a => a.UploadTime)
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private static void ConfigureTicketLog(ModelBuilder builder)
    {
        builder.Entity<TicketLog>(log =>
        {
            log.ToTable("TicketLogs");

            log.Property(l => l.Action)
                .IsRequired()
                .HasMaxLength(150);

            log.Property(l => l.Notes)
                .HasMaxLength(1000);

            log.Property(l => l.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            log.HasOne(l => l.Ticket)
                .WithMany(t => t.Logs)
                .HasForeignKey(l => l.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            log.HasOne(l => l.PerformedBy)
                .WithMany()
                .HasForeignKey(l => l.PerformedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAccessRequest(ModelBuilder builder)
    {
        builder.Entity<AccessRequest>(accessRequest =>
        {
            accessRequest.ToTable("AccessRequests");

            accessRequest.Property(ar => ar.FullName)
                .IsRequired()
                .HasMaxLength(150);

            accessRequest.Property(ar => ar.EmployeeNumber)
                .IsRequired()
                .HasMaxLength(50);

            accessRequest.Property(ar => ar.Department)
                .IsRequired()
                .HasMaxLength(100);

            accessRequest.Property(ar => ar.Email)
                .IsRequired();

            accessRequest.Property(ar => ar.PhoneNumber)
                .HasMaxLength(50);

            accessRequest.Property(ar => ar.AccessType)
                .HasConversion<string>()
                .HasMaxLength(20);

            accessRequest.Property(ar => ar.SystemName)
                .IsRequired()
                .HasMaxLength(150);

            accessRequest.Property(ar => ar.Reason)
                .IsRequired()
                .HasMaxLength(1000);

            accessRequest.Property(ar => ar.AccessDuration)
                .HasMaxLength(100);

            accessRequest.Property(ar => ar.ManagerApprovalName)
                .HasMaxLength(150);

            accessRequest.Property(ar => ar.ManagerApprovalStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            accessRequest.Property(ar => ar.ITApprovalName)
                .HasMaxLength(150);

            accessRequest.Property(ar => ar.ITApprovalStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            accessRequest.Property(ar => ar.SecurityApprovalName)
                .HasMaxLength(150);

            accessRequest.Property(ar => ar.SecurityApprovalStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            accessRequest.Property(ar => ar.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            accessRequest.Property(ar => ar.SelectedManagerId)
                .IsRequired()
                .HasMaxLength(450);

            // One-to-One relationship with Ticket
            accessRequest.HasOne(ar => ar.Ticket)
                .WithOne()
                .HasForeignKey<AccessRequest>(ar => ar.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with Selected Manager
            accessRequest.HasOne(ar => ar.SelectedManager)
                .WithMany()
                .HasForeignKey(ar => ar.SelectedManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Make TicketId unique for one-to-one relationship
            accessRequest.HasIndex(ar => ar.TicketId)
                .IsUnique();
        });
    }

    private static void ConfigureServiceRequest(ModelBuilder builder)
    {
        builder.Entity<ServiceRequest>(serviceRequest =>
        {
            serviceRequest.ToTable("ServiceRequests");

            serviceRequest.Property(sr => sr.EmployeeName)
                .IsRequired()
                .HasMaxLength(150);

            serviceRequest.Property(sr => sr.Department)
                .IsRequired()
                .HasMaxLength(100);

            serviceRequest.Property(sr => sr.JobTitle)
                .IsRequired()
                .HasMaxLength(100);

            serviceRequest.Property(sr => sr.UsageDescription)
                .IsRequired()
                .HasMaxLength(2000);

            serviceRequest.Property(sr => sr.UsageReason)
                .IsRequired()
                .HasMaxLength(2000);

            serviceRequest.Property(sr => sr.SignatureName)
                .IsRequired()
                .HasMaxLength(150);

            serviceRequest.Property(sr => sr.SignatureJobTitle)
                .IsRequired()
                .HasMaxLength(100);

            serviceRequest.Property(sr => sr.ManagerApprovalName)
                .HasMaxLength(150);

            serviceRequest.Property(sr => sr.ManagerApprovalStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            serviceRequest.Property(sr => sr.ITApprovalName)
                .HasMaxLength(150);

            serviceRequest.Property(sr => sr.ITApprovalStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            serviceRequest.Property(sr => sr.SecurityApprovalName)
                .HasMaxLength(150);

            serviceRequest.Property(sr => sr.SecurityApprovalStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            serviceRequest.Property(sr => sr.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            serviceRequest.Property(sr => sr.SelectedManagerId)
                .IsRequired()
                .HasMaxLength(450);

            // One-to-One relationship with Ticket
            serviceRequest.HasOne(sr => sr.Ticket)
                .WithOne()
                .HasForeignKey<ServiceRequest>(sr => sr.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with Selected Manager
            serviceRequest.HasOne(sr => sr.SelectedManager)
                .WithMany()
                .HasForeignKey(sr => sr.SelectedManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Make TicketId unique for one-to-one relationship
            serviceRequest.HasIndex(sr => sr.TicketId)
                .IsUnique();
        });
    }

    private static void ConfigureAssets(ModelBuilder builder)
    {
        // Configure base Asset entity with TPH (Table Per Hierarchy)
        builder.Entity<Asset>(asset =>
        {
            asset.ToTable("Assets");

            asset.HasDiscriminator<string>("AssetType")
                .HasValue<AccessPoint>("AccessPoint")
                .HasValue<Computer>("Computer")
                .HasValue<Server>("Server")
                .HasValue<VirtualHost>("VirtualHost")
                .HasValue<VirtualMachine>("VirtualMachine")
                .HasValue<Workstation>("Workstation")
                .HasValue<MobileDevice>("MobileDevice")
                .HasValue<Smartphone>("Smartphone")
                .HasValue<Tablet>("Tablet")
                .HasValue<Printer>("Printer")
                .HasValue<Router>("Router")
                .HasValue<CiscoRouter>("CiscoRouter")
                .HasValue<Switch>("Switch")
                .HasValue<CiscoCatosSwitch>("CiscoCatosSwitch")
                .HasValue<CiscoSwitch>("CiscoSwitch");

            asset.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(150);

            asset.Property(a => a.SerialNumber)
                .HasMaxLength(100);

            asset.Property(a => a.AssetTag)
                .HasMaxLength(50);

            asset.Property(a => a.Location)
                .HasMaxLength(150);

            asset.Property(a => a.PurchaseCost)
                .HasPrecision(18, 2);

            asset.Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            asset.HasOne(a => a.Product)
                .WithMany()
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            asset.HasOne(a => a.Vendor)
                .WithMany()
                .HasForeignKey(a => a.VendorId)
                .OnDelete(DeleteBehavior.SetNull);

            asset.HasOne(a => a.AssetState)
                .WithMany()
                .HasForeignKey(a => a.AssetStateId)
                .OnDelete(DeleteBehavior.Cascade);

            asset.HasOne(a => a.NetworkDetails)
                .WithMany()
                .HasForeignKey(a => a.NetworkDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            asset.HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Product
        builder.Entity<Product>(product =>
        {
            product.ToTable("Products");

            product.Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(150);

            product.Property(p => p.Manufacturer)
                .IsRequired()
                .HasMaxLength(100);

            product.Property(p => p.ProductType)
                .IsRequired()
                .HasMaxLength(100);

            product.Property(p => p.PartNo)
                .HasMaxLength(50);

            product.Property(p => p.Cost)
                .HasPrecision(18, 2);

            product.Property(p => p.Description)
                .HasMaxLength(500);
        });

        // Configure Vendor
        builder.Entity<Vendor>(vendor =>
        {
            vendor.ToTable("Vendors");

            vendor.Property(v => v.VendorName)
                .IsRequired()
                .HasMaxLength(150);

            vendor.Property(v => v.Currency)
                .IsRequired()
                .HasMaxLength(5);
        });

        // Configure AssetState
        builder.Entity<AssetState>(assetState =>
        {
            assetState.ToTable("AssetStates");

            assetState.Property(s => s.Status)
                .HasConversion<int>();
        });

        // Configure NetworkDetails
        builder.Entity<NetworkDetails>(network =>
        {
            network.ToTable("NetworkDetails");
        });

        // Configure ComputerInfo
        builder.Entity<ComputerInfo>(computerInfo =>
        {
            computerInfo.ToTable("ComputerInfos");
        });

        // Configure OperatingSystemInfo
        builder.Entity<OperatingSystemInfo>(osInfo =>
        {
            osInfo.ToTable("OperatingSystemInfos");
        });

        // Configure MemoryDetails
        builder.Entity<MemoryDetails>(memory =>
        {
            memory.ToTable("MemoryDetails");
        });

        // Configure Processor
        builder.Entity<Processor>(processor =>
        {
            processor.ToTable("Processors");
        });

        // Configure HardDisk
        builder.Entity<HardDisk>(disk =>
        {
            disk.ToTable("HardDisks");
        });

        // Configure Keyboard
        builder.Entity<Keyboard>(keyboard =>
        {
            keyboard.ToTable("Keyboards");
        });

        // Configure Mouse
        builder.Entity<Mouse>(mouse =>
        {
            mouse.ToTable("Mice");
        });

        // Configure Monitor
        builder.Entity<Monitor>(monitor =>
        {
            monitor.ToTable("Monitors");
        });

        // Configure MobileDetails
        builder.Entity<MobileDetails>(mobile =>
        {
            mobile.ToTable("MobileDetails");
        });

        // Configure Computer entity relationships
        builder.Entity<Computer>(computer =>
        {
            // Map properties to columns without prefix
            computer.Property(c => c.ComputerInfoId).HasColumnName("ComputerInfoId");
            computer.Property(c => c.OperatingSystemInfoId).HasColumnName("OperatingSystemInfoId");
            computer.Property(c => c.MemoryDetailsId).HasColumnName("MemoryDetailsId");
            computer.Property(c => c.ProcessorId).HasColumnName("ProcessorId");
            computer.Property(c => c.HardDiskId).HasColumnName("HardDiskId");
            computer.Property(c => c.KeyboardId).HasColumnName("KeyboardId");
            computer.Property(c => c.MouseId).HasColumnName("MouseId");
            computer.Property(c => c.MonitorId).HasColumnName("MonitorId");

            computer.HasOne(c => c.ComputerInfo)
                .WithMany()
                .HasForeignKey(c => c.ComputerInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            computer.HasOne(c => c.OperatingSystemInfo)
                .WithMany()
                .HasForeignKey(c => c.OperatingSystemInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            computer.HasOne(c => c.MemoryDetails)
                .WithMany()
                .HasForeignKey(c => c.MemoryDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            computer.HasOne(c => c.Processor)
                .WithMany()
                .HasForeignKey(c => c.ProcessorId)
                .OnDelete(DeleteBehavior.Cascade);

            computer.HasOne(c => c.HardDisk)
                .WithMany()
                .HasForeignKey(c => c.HardDiskId)
                .OnDelete(DeleteBehavior.Cascade);

            computer.HasOne(c => c.Keyboard)
                .WithMany()
                .HasForeignKey(c => c.KeyboardId)
                .OnDelete(DeleteBehavior.Cascade);

            computer.HasOne(c => c.Mouse)
                .WithMany()
                .HasForeignKey(c => c.MouseId)
                .OnDelete(DeleteBehavior.Cascade);

            computer.HasOne(c => c.Monitor)
                .WithMany()
                .HasForeignKey(c => c.MonitorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Server entity relationships
        builder.Entity<Server>(server =>
        {
            // Map properties to columns without prefix
            server.Property(s => s.ComputerInfoId).HasColumnName("ComputerInfoId");
            server.Property(s => s.OperatingSystemInfoId).HasColumnName("OperatingSystemInfoId");
            server.Property(s => s.MemoryDetailsId).HasColumnName("MemoryDetailsId");
            server.Property(s => s.ProcessorId).HasColumnName("ProcessorId");
            server.Property(s => s.HardDiskId).HasColumnName("HardDiskId");
            server.Property(s => s.KeyboardId).HasColumnName("KeyboardId");
            server.Property(s => s.MouseId).HasColumnName("MouseId");
            server.Property(s => s.MonitorId).HasColumnName("MonitorId");

            server.HasOne(s => s.ComputerInfo)
                .WithMany()
                .HasForeignKey(s => s.ComputerInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            server.HasOne(s => s.OperatingSystemInfo)
                .WithMany()
                .HasForeignKey(s => s.OperatingSystemInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            server.HasOne(s => s.MemoryDetails)
                .WithMany()
                .HasForeignKey(s => s.MemoryDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            server.HasOne(s => s.Processor)
                .WithMany()
                .HasForeignKey(s => s.ProcessorId)
                .OnDelete(DeleteBehavior.Cascade);

            server.HasOne(s => s.HardDisk)
                .WithMany()
                .HasForeignKey(s => s.HardDiskId)
                .OnDelete(DeleteBehavior.Cascade);

            server.HasOne(s => s.Keyboard)
                .WithMany()
                .HasForeignKey(s => s.KeyboardId)
                .OnDelete(DeleteBehavior.Cascade);

            server.HasOne(s => s.Mouse)
                .WithMany()
                .HasForeignKey(s => s.MouseId)
                .OnDelete(DeleteBehavior.Cascade);

            server.HasOne(s => s.Monitor)
                .WithMany()
                .HasForeignKey(s => s.MonitorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure VirtualHost entity relationships
        builder.Entity<VirtualHost>(vh =>
        {
            vh.Property(v => v.VMPlatform)
                .HasMaxLength(100);

            // Map properties to columns without prefix
            vh.Property(v => v.ComputerInfoId).HasColumnName("ComputerInfoId");
            vh.Property(v => v.OperatingSystemInfoId).HasColumnName("OperatingSystemInfoId");
            vh.Property(v => v.MemoryDetailsId).HasColumnName("MemoryDetailsId");
            vh.Property(v => v.ProcessorId).HasColumnName("ProcessorId");
            vh.Property(v => v.HardDiskId).HasColumnName("HardDiskId");
            vh.Property(v => v.KeyboardId).HasColumnName("KeyboardId");
            vh.Property(v => v.MouseId).HasColumnName("MouseId");
            vh.Property(v => v.MonitorId).HasColumnName("MonitorId");

            vh.HasOne(v => v.ComputerInfo)
                .WithMany()
                .HasForeignKey(v => v.ComputerInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            vh.HasOne(v => v.OperatingSystemInfo)
                .WithMany()
                .HasForeignKey(v => v.OperatingSystemInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            vh.HasOne(v => v.MemoryDetails)
                .WithMany()
                .HasForeignKey(v => v.MemoryDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            vh.HasOne(v => v.Processor)
                .WithMany()
                .HasForeignKey(v => v.ProcessorId)
                .OnDelete(DeleteBehavior.Cascade);

            vh.HasOne(v => v.HardDisk)
                .WithMany()
                .HasForeignKey(v => v.HardDiskId)
                .OnDelete(DeleteBehavior.Cascade);

            vh.HasOne(v => v.Keyboard)
                .WithMany()
                .HasForeignKey(v => v.KeyboardId)
                .OnDelete(DeleteBehavior.Cascade);

            vh.HasOne(v => v.Mouse)
                .WithMany()
                .HasForeignKey(v => v.MouseId)
                .OnDelete(DeleteBehavior.Cascade);

            vh.HasOne(v => v.Monitor)
                .WithMany()
                .HasForeignKey(v => v.MonitorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure VirtualMachine entity relationships
        builder.Entity<VirtualMachine>(vm =>
        {
            vm.HasOne(v => v.VirtualHost)
                .WithMany()
                .HasForeignKey(v => v.VirtualHostId)
                .OnDelete(DeleteBehavior.Restrict);

            vm.HasOne(v => v.ComputerInfo)
                .WithMany()
                .HasForeignKey(v => v.ComputerInfoId)
                .OnDelete(DeleteBehavior.NoAction);

            vm.HasOne(v => v.OperatingSystemInfo)
                .WithMany()
                .HasForeignKey(v => v.OperatingSystemInfoId)
                .OnDelete(DeleteBehavior.NoAction);

            vm.HasOne(v => v.MemoryDetails)
                .WithMany()
                .HasForeignKey(v => v.MemoryDetailsId)
                .OnDelete(DeleteBehavior.NoAction);

            vm.HasOne(v => v.Processor)
                .WithMany()
                .HasForeignKey(v => v.ProcessorId)
                .OnDelete(DeleteBehavior.NoAction);

            vm.HasOne(v => v.HardDisk)
                .WithMany()
                .HasForeignKey(v => v.HardDiskId)
                .OnDelete(DeleteBehavior.NoAction);

            vm.HasOne(v => v.Keyboard)
                .WithMany()
                .HasForeignKey(v => v.KeyboardId)
                .OnDelete(DeleteBehavior.NoAction);

            vm.HasOne(v => v.Mouse)
                .WithMany()
                .HasForeignKey(v => v.MouseId)
                .OnDelete(DeleteBehavior.NoAction);

            vm.HasOne(v => v.Monitor)
                .WithMany()
                .HasForeignKey(v => v.MonitorId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure Workstation entity relationships
        builder.Entity<Workstation>(ws =>
        {
            // Map properties to columns without prefix
            ws.Property(w => w.ComputerInfoId).HasColumnName("ComputerInfoId");
            ws.Property(w => w.OperatingSystemInfoId).HasColumnName("OperatingSystemInfoId");
            ws.Property(w => w.MemoryDetailsId).HasColumnName("MemoryDetailsId");
            ws.Property(w => w.ProcessorId).HasColumnName("ProcessorId");
            ws.Property(w => w.HardDiskId).HasColumnName("HardDiskId");
            ws.Property(w => w.KeyboardId).HasColumnName("KeyboardId");
            ws.Property(w => w.MouseId).HasColumnName("MouseId");
            ws.Property(w => w.MonitorId).HasColumnName("MonitorId");

            ws.HasOne(w => w.ComputerInfo)
                .WithMany()
                .HasForeignKey(w => w.ComputerInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            ws.HasOne(w => w.OperatingSystemInfo)
                .WithMany()
                .HasForeignKey(w => w.OperatingSystemInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            ws.HasOne(w => w.MemoryDetails)
                .WithMany()
                .HasForeignKey(w => w.MemoryDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            ws.HasOne(w => w.Processor)
                .WithMany()
                .HasForeignKey(w => w.ProcessorId)
                .OnDelete(DeleteBehavior.Cascade);

            ws.HasOne(w => w.HardDisk)
                .WithMany()
                .HasForeignKey(w => w.HardDiskId)
                .OnDelete(DeleteBehavior.Cascade);

            ws.HasOne(w => w.Keyboard)
                .WithMany()
                .HasForeignKey(w => w.KeyboardId)
                .OnDelete(DeleteBehavior.Cascade);

            ws.HasOne(w => w.Mouse)
                .WithMany()
                .HasForeignKey(w => w.MouseId)
                .OnDelete(DeleteBehavior.Cascade);

            ws.HasOne(w => w.Monitor)
                .WithMany()
                .HasForeignKey(w => w.MonitorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MobileDevice entity relationships
        builder.Entity<MobileDevice>(md =>
        {
            // Map properties to columns without prefix
            md.Property(m => m.MobileDetailsId).HasColumnName("MobileDetailsId");
            md.Property(m => m.OperatingSystemInfoId).HasColumnName("OperatingSystemInfoId");

            md.HasOne(m => m.MobileDetails)
                .WithMany()
                .HasForeignKey(m => m.MobileDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            md.HasOne(m => m.OperatingSystemInfo)
                .WithMany()
                .HasForeignKey(m => m.OperatingSystemInfoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
