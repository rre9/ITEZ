using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// معلومات الفأرة
/// </summary>
public class Mouse
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? MouseType { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    public int? MouseButtons { get; set; }

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
