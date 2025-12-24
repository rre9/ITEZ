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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureTicket(builder);
        ConfigureTicketAttachment(builder);
        ConfigureTicketLog(builder);
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
}
