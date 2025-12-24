using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Data;

public class ApplicationDbContext : IdentityDbContext
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

        var ticket = builder.Entity<Ticket>();
        ticket.Property(t => t.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        ticket.HasIndex(t => t.Status);
        ticket.HasIndex(t => t.Priority);
        ticket.HasIndex(t => t.Department);
        ticket.HasIndex(t => t.CreatedAt);
        ticket.HasIndex(t => t.CreatedById);
        ticket.HasIndex(t => t.AssignedToId);

        ticket.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        ticket.HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        ticket.HasMany(t => t.Attachments)
            .WithOne(a => a.Ticket)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        ticket.HasMany(t => t.Logs)
            .WithOne(l => l.Ticket)
            .HasForeignKey(l => l.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TicketAttachment>()
            .HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TicketLog>()
            .HasOne(l => l.CreatedBy)
            .WithMany()
            .HasForeignKey(l => l.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
