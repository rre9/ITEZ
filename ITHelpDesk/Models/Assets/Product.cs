using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// نموذج المنتج - يحتوي على معلومات المنتج العام (أكسس بوينت، كمبيوتر، إلخ)
/// </summary>
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProductType { get; set; } = default!; // Access Point, Computer, Server, etc.

    [Required]
    [MaxLength(150)]
    public string ProductName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string Manufacturer { get; set; } = default!;

    [MaxLength(50)]
    public string? PartNo { get; set; }

    public decimal Cost { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
