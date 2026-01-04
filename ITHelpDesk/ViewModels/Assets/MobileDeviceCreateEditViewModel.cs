using ITHelpDesk.Models.Assets;

namespace ITHelpDesk.ViewModels.Assets
{
    public class MobileDeviceCreateEditViewModel : AssetCreateEditViewModel
    {
        // Mobile-specific fields
        public int? MobileDetailsId { get; set; }

        public string IMEI { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public long? StorageCapacityGB { get; set; }
        public string OSPlatform { get; set; } // "iOS", "Android", "Windows"
        public string OSVersion { get; set; }
        public string PhoneNumber { get; set; }
        public string SIMProvider { get; set; }
        public DateTime? SIMActivationDate { get; set; }
    }
}
