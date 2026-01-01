namespace ITHelpDesk.ViewModels.Assets
{
    public class NetworkEquipmentCreateEditViewModel : AssetCreateEditViewModel
    {
        // Network Equipment (Router, Switch) specific fields
        public string Model { get; set; }
        public int? PortCount { get; set; }
        public string PortType { get; set; } // "Ethernet", "Fiber", "Mixed"
        public string ManagementIP { get; set; }
        public string FirmwareVersion { get; set; }
        public DateTime? FirmwareUpdateDate { get; set; }
        public bool? IsStackable { get; set; }
    }
}
