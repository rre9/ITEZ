using ITHelpDesk.Models.Assets;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.ViewModels.Assets
{
    public class ServerCreateViewModel
    {
        // Basic Information
        public int? Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string? SerialNumber { get; set; }

        [StringLength(50)]
        public string? AssetTag { get; set; }

        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        public int? VendorId { get; set; }

        [DataType(DataType.Currency)]
        public decimal? PurchaseCost { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? WarrantyExpiryDate { get; set; }

        // Computer Info
        public string? ServiceTag { get; set; }
        public string? ComputerManufacturer { get; set; }
        public DateTime? BiosDate { get; set; }
        public string? Domain { get; set; }
        public string? SMBiosVersion { get; set; }
        public string? BiosVersion { get; set; }
        public string? BiosManufacturer { get; set; }

        // Operating System Info
        public string? OSName { get; set; }
        public string? OSVersion { get; set; }
        public string? BuildNumber { get; set; }
        public string? ServicePack { get; set; }
        public string? OSProductId { get; set; }

        // Memory Details
        public int? RAM { get; set; }
        public int? VirtualMemory { get; set; }

        // Processor
        public string? ProcessorInfo { get; set; }
        public string? ProcessorManufacturer { get; set; }
        public int? ClockSpeedMHz { get; set; }
        public int? NumberOfCores { get; set; }

        // Hard Disk
        public string? HardDiskModel { get; set; }
        public string? HardDiskSerialNumber { get; set; }
        public string? HardDiskManufacturer { get; set; }
        public int? CapacityGB { get; set; }

        // Keyboard
        public string? KeyboardType { get; set; }
        public string? KeyboardManufacturer { get; set; }
        public string? KeyboardSerialNumber { get; set; }

        // Mouse
        public string? MouseType { get; set; }
        public string? MouseSerialNumber { get; set; }
        public int? MouseButtons { get; set; }
        public string? MouseManufacturer { get; set; }

        // Monitor
        public string? MonitorType { get; set; }
        public string? MonitorSerialNumber { get; set; }
        public string? MonitorManufacturer { get; set; }
        public string? MaxResolution { get; set; }

        // Asset State
        public AssetStatusEnum Status { get; set; }
        public string? AssociatedTo { get; set; }
        public string? Site { get; set; }
        public string? StateComments { get; set; }
        public string? UserId { get; set; }
        public string? Department { get; set; }

        // Additional Fields
        public DateTime? ExpiryDate { get; set; }
        public DateTime? AcquisitionDate { get; set; }

        // Network Details
        public new string? IPAddress { get; set; }
        public new string? MACAddress { get; set; }
        public string? NIC { get; set; }
        public string? Network { get; set; }
        public string? DefaultGateway { get; set; }
        public new bool DHCPEnabled { get; set; }
        public string? DHCPServer { get; set; }
        public string? DNSHostname { get; set; }
    }
}
