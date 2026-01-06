using System;
using System.ComponentModel.DataAnnotations;
using ITHelpDesk.Models.Assets;

namespace ITHelpDesk.ViewModels.Assets;

/// <summary>
/// ViewModel لإنشاء راوتر جديد
/// </summary>
public class RouterCreateViewModel
{
    // Basic Information
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product is required")]
    public int ProductId { get; set; }

    // Asset Details
    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    [MaxLength(50)]
    public string? AssetTag { get; set; }

    public int? VendorId { get; set; }

    public decimal PurchaseCost { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(150)]
    public string? Location { get; set; }

    public DateTime? AcquisitionDate { get; set; }

    public DateTime? WarrantyExpiryDate { get; set; }

    // Asset State
    [Required(ErrorMessage = "Asset Status is required")]
    public AssetStatusEnum AssetStatus { get; set; } = AssetStatusEnum.InStore;

    [MaxLength(50)]
    public string? AssociatedTo { get; set; }

    [MaxLength(50)]
    public string? Site { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [Required(ErrorMessage = "Department is required")]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? StateComments { get; set; }

    // Network Details
    [MaxLength(15)]
    public string? IPAddress { get; set; }

    [MaxLength(17)]
    public string? MACAddress { get; set; }

    [MaxLength(50)]
    public string? NIC { get; set; }

    [MaxLength(100)]
    public string? Network { get; set; }

    [MaxLength(15)]
    public string? DefaultGateway { get; set; }

    public bool DHCPEnabled { get; set; } = false;

    [MaxLength(15)]
    public string? DHCPServer { get; set; }

    [MaxLength(100)]
    public string? DNSHostname { get; set; }
}
