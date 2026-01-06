using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.ViewModels.Assets;

public class MobileCreateViewModel
{
    // Basic Info
    [Required(ErrorMessage = "Name is required")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product is required")]
    public int ProductId { get; set; }

    // Asset Details
    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [StringLength(50)]
    public string? AssetTag { get; set; }

    public int? VendorId { get; set; }

    public decimal PurchaseCost { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [StringLength(150)]
    public string? Location { get; set; }

    public DateTime? AcquisitionDate { get; set; }

    public DateTime? WarrantyExpiryDate { get; set; }

    // Mobile Details
    [StringLength(20)]
    public string? IMEI { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    [StringLength(100)]
    public string? ModelNo { get; set; }

    public int? TotalCapacityGB { get; set; }

    public int? AvailableCapacityGB { get; set; }

    [StringLength(100)]
    public string? ModemFirmwareVersion { get; set; }

    // Operating System Info
    [StringLength(100)]
    public string? OSName { get; set; }

    [StringLength(50)]
    public string? OSVersion { get; set; }

    [StringLength(50)]
    public string? BuildNumber { get; set; }

    [StringLength(50)]
    public string? ServicePack { get; set; }

    [StringLength(100)]
    public string? OSProductId { get; set; }

    // Asset State
    public int AssetStatus { get; set; }

    [StringLength(50)]
    public string? AssociatedTo { get; set; }

    [StringLength(50)]
    public string? Site { get; set; }

    [StringLength(500)]
    public string? StateComments { get; set; }

    public string? UserId { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    // Network Details
    [StringLength(15)]
    public string? IPAddress { get; set; }

    [StringLength(17)]
    public string? MACAddress { get; set; }

    [StringLength(50)]
    public string? NIC { get; set; }

    [StringLength(100)]
    public string? Network { get; set; }

    [StringLength(15)]
    public string? DefaultGateway { get; set; }

    public bool DHCPEnabled { get; set; }

    [StringLength(15)]
    public string? DHCPServer { get; set; }

    [StringLength(100)]
    public string? DNSHostname { get; set; }
}
