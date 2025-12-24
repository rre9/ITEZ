using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ITHelpDesk.Models;

public class TicketAttachment
{
    public int Id { get; set; }

    [Required]
    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = default!;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = default!;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = default!;

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string UploadedById { get; set; } = default!;

    public IdentityUser? UploadedBy { get; set; }
}

