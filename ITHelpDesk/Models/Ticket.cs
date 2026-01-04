using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models;

public class Ticket
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = default!;

    [Required]
    public string Description { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = default!;

    [Required]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    [Required]
    public TicketStatus Status { get; set; } = TicketStatus.New;

    [Required]
    public string CreatedById { get; set; } = default!;

    public ApplicationUser CreatedBy { get; set; } = default!;

    public string? AssignedToId { get; set; }

    public ApplicationUser? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CloseReason? CloseReason { get; set; }

    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();

    public ICollection<TicketLog> Logs { get; set; } = new List<TicketLog>();
}

