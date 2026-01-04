using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.ViewModels.Assets
{
    public class PrinterCreateEditViewModel
    {
        // Basic Information
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        // Asset Details
        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [StringLength(50)]
        public string? AssetTag { get; set; }

        public int? VendorId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? PurchaseCost { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public DateTime? AcquisitionDate { get; set; }

        public DateTime? WarrantyExpiryDate { get; set; }

        // Asset State
        public int AssetStatus { get; set; } = 1; // Default: In Use

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
        [StringLength(45)]
        public string? IPAddress { get; set; }

        [StringLength(17)]
        public string? MACAddress { get; set; }

        [StringLength(100)]
        public string? NIC { get; set; }

        [StringLength(100)]
        public string? Network { get; set; }

        [StringLength(45)]
        public string? DefaultGateway { get; set; }

        public bool? DHCPEnabled { get; set; }

        [StringLength(45)]
        public string? DHCPServer { get; set; }

        [StringLength(100)]
        public string? DNSHostname { get; set; }
    }
}
