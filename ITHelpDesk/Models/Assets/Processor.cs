using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// معلومات المعالج
/// </summary>
public class Processor
{
    [Key]
    public int Id { get; set; }

    [MaxLength(150)]
    public string? ProcessorInfo { get; set; }

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    public int? ClockSpeedMHz { get; set; }

    public int? NumberOfCores { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
