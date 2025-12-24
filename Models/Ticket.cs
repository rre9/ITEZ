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
    [MaxLength(50)]
    public string Status { get; set; } = "Open";

    [Required]
    [MaxLength(50)]
    public string Priority { get; set; } = "Normal";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Required]
    public string CreatedById { get; set; } = default!;

    public ApplicationUser? CreatedBy { get; set; }

    public string? AssignedToId { get; set; }

    public ApplicationUser? AssignedTo { get; set; }

    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();

    public ICollection<TicketLog> Logs { get; set; } = new List<TicketLog>();
}

