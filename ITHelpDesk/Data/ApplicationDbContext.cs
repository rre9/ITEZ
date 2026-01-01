using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureTicket(builder);
        ConfigureTicketAttachment(builder);
        ConfigureTicketLog(builder);
        ConfigureAccessRequest(builder);
        ConfigureServiceRequest(builder);
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
}
