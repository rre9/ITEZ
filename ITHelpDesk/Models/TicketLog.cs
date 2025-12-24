using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models;

public class TicketLog
{
    public int Id { get; set; }

    [Required]
    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = default!;

    [Required]
    [MaxLength(150)]
    public string Action { get; set; } = default!;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public string PerformedById { get; set; } = default!;

    public ApplicationUser PerformedBy { get; set; } = default!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

