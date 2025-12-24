using System;
using System.ComponentModel.DataAnnotations;

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

    public DateTime UploadTime { get; set; } = DateTime.UtcNow;
}

