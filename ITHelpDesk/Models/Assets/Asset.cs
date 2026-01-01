using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// الفئة الأساسية لجميع الأصول
/// </summary>
public abstract class Asset
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = default!;

    [Required]
    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = default!;

    // Asset Details
    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    [MaxLength(50)]
    public string? AssetTag { get; set; }

    public int? VendorId { get; set; }

    [ForeignKey(nameof(VendorId))]
    public Vendor? Vendor { get; set; }

    public decimal PurchaseCost { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(150)]
    public string? Location { get; set; }

    public DateTime? AcquisitionDate { get; set; }

    public DateTime? WarrantyExpiryDate { get; set; }

    // Asset State
    public int? AssetStateId { get; set; }

    [ForeignKey(nameof(AssetStateId))]
    public AssetState? AssetState { get; set; }

    // Network Details
    public int? NetworkDetailsId { get; set; }

    [ForeignKey(nameof(NetworkDetailsId))]
    public NetworkDetails? NetworkDetails { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [MaxLength(450)]
    public string? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public ApplicationUser? CreatedBy { get; set; }
}
