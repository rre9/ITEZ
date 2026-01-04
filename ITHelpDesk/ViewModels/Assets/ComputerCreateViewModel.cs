using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.ViewModels.Assets;

public class ComputerCreateViewModel
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

    // Computer Info
    [StringLength(50)]
    public string? ServiceTag { get; set; }

    [StringLength(100)]
    public string? ComputerManufacturer { get; set; }

    public DateTime? BiosDate { get; set; }

    [StringLength(100)]
    public string? Domain { get; set; }

    [StringLength(50)]
    public string? SMBiosVersion { get; set; }

    [StringLength(50)]
    public string? BiosVersion { get; set; }

    [StringLength(100)]
    public string? BiosManufacturer { get; set; }

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

    // Memory Details
    public int? RAM { get; set; }

    public int? VirtualMemory { get; set; }

    // Processor
    [StringLength(150)]
    public string? ProcessorInfo { get; set; }

    [StringLength(100)]
    public string? ProcessorManufacturer { get; set; }

    public int? ClockSpeedMHz { get; set; }

    public int? NumberOfCores { get; set; }

    // Hard Disk
    [StringLength(100)]
    public string? HardDiskModel { get; set; }

    [StringLength(100)]
    public string? HardDiskSerialNumber { get; set; }

    [StringLength(100)]
    public string? HardDiskManufacturer { get; set; }

    public int? CapacityGB { get; set; }

    // Keyboard
    [StringLength(100)]
    public string? KeyboardType { get; set; }

    [StringLength(100)]
    public string? KeyboardManufacturer { get; set; }

    [StringLength(100)]
    public string? KeyboardSerialNumber { get; set; }

    // Mouse
    [StringLength(100)]
    public string? MouseType { get; set; }

    [StringLength(100)]
    public string? MouseSerialNumber { get; set; }

    public int? MouseButtons { get; set; }

    [StringLength(100)]
    public string? MouseManufacturer { get; set; }

    // Monitor
    [StringLength(100)]
    public string? MonitorType { get; set; }

    [StringLength(100)]
    public string? MonitorSerialNumber { get; set; }

    [StringLength(100)]
    public string? MonitorManufacturer { get; set; }

    [StringLength(100)]
    public string? MaxResolution { get; set; }

    // Asset State
    public int AssetStatus { get; set; } = 1;

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
