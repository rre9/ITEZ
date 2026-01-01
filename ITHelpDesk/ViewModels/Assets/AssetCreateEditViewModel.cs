using ITHelpDesk.Models.Assets;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.ViewModels.Assets
{
    public class AssetCreateEditViewModel
    {
        public int? Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string SerialNumber { get; set; }

        [StringLength(50)]
        public string AssetTag { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? VendorId { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Cost { get; set; }

        [DataType(DataType.Currency)]
        public decimal? PurchaseCost { get; set; }

        [StringLength(255)]
        public string Location { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? WarrantyExpiryDate { get; set; }

        [Required]
        public int AssetStateId { get; set; }

        // Network Details
        [StringLength(45)]
        public string IPAddress { get; set; }

        [StringLength(17)]
        public string MACAddress { get; set; }

        [StringLength(45)]
        public string Gateway { get; set; }

        public bool? DHCPEnabled { get; set; }

        [StringLength(45)]
        public string DNSServer { get; set; }

        public IEnumerable<ProductDropdownItem> Products { get; set; } = new List<ProductDropdownItem>();
        public IEnumerable<VendorDropdownItem> Vendors { get; set; } = new List<VendorDropdownItem>();
        public IEnumerable<AssetStateDropdownItem> AssetStates { get; set; } = new List<AssetStateDropdownItem>();

        public bool IsEdit => Id.HasValue;
        public string AssetType { get; set; } // "AccessPoint", "Computer", etc.
    }

    public class ProductDropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
    }

    public class VendorDropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AssetStateDropdownItem
    {
        public int Id { get; set; }
        public string State { get; set; }
    }
}
