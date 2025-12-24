using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ITHelpDesk.Models;

public class TicketLog
{
    public int Id { get; set; }

    [Required]
    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = default!;

    [Required]
    [MaxLength(50)]
    public string LogType { get; set; } = default!;

    [Required]
    public string Message { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string CreatedById { get; set; } = default!;

    public IdentityUser? CreatedBy { get; set; }
}

