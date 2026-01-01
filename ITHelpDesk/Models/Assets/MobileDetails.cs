using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// معلومات الجهاز المحمول (الهاتف والتابلت)
/// </summary>
public class MobileDetails
{
    [Key]
    public int Id { get; set; }

    [MaxLength(20)]
    public string? IMEI { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(100)]
    public string? ModelNo { get; set; }

    public int? TotalCapacityGB { get; set; }

    public int? AvailableCapacityGB { get; set; }

    [MaxLength(100)]
    public string? ModemFirmwareVersion { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
