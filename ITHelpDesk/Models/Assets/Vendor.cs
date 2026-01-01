using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// نموذج البائع - معلومات الجهات البائعة للمنتجات
/// </summary>
public class Vendor
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string VendorName { get; set; } = default!;

    [Required]
    [MaxLength(5)]
    public string Currency { get; set; } = "SR"; // Always SR

    [MaxLength(10)]
    public string? DoorNumber { get; set; }

    [MaxLength(100)]
    public string? Landmark { get; set; }

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    [MaxLength(50)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? Fax { get; set; }

    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? Street { get; set; }

    [MaxLength(50)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? StateProvince { get; set; }

    [MaxLength(20)]
    public string? PhoneNo { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
