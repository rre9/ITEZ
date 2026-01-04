using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// معلومات نظام التشغيل
/// </summary>
public class OperatingSystemInfo
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? Version { get; set; }

    [MaxLength(50)]
    public string? BuildNumber { get; set; }

    [MaxLength(50)]
    public string? ServicePack { get; set; }

    [MaxLength(100)]
    public string? ProductId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
