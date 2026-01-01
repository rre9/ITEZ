using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// معلومات الكمبيوتر BIOS والمعالج وغيره
/// </summary>
public class ComputerInfo
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string? ServiceTag { get; set; }

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    public DateTime? BiosDate { get; set; }

    [MaxLength(100)]
    public string? Domain { get; set; }

    [MaxLength(50)]
    public string? SMBiosVersion { get; set; }

    [MaxLength(50)]
    public string? BiosVersion { get; set; }

    [MaxLength(100)]
    public string? BiosManufacturer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
