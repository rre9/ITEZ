using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// حالة الأصل (الجهاز) - في المخزن، قيد الاستخدام، في الإصلاح، إلخ
/// </summary>
public enum AssetStatusEnum
{
    InStore = 1,
    InUse = 2,
    InRepair = 3,
    Expired = 4,
    Disposed = 5
}

/// <summary>
/// معلومات حالة الأصل
/// </summary>
public class AssetState
{
    [Key]
    public int Id { get; set; }

    [Required]
    public AssetStatusEnum Status { get; set; } = AssetStatusEnum.InStore;

    [MaxLength(50)]
    public string? AssociatedTo { get; set; } // User, Department, etc.

    [MaxLength(50)]
    public string? Site { get; set; }

    [MaxLength(500)]
    public string? StateComments { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
