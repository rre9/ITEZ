using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// معلومات الشاشة
/// </summary>
public class Monitor
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? MonitorType { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    [MaxLength(100)]
    public string? MaxResolution { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
