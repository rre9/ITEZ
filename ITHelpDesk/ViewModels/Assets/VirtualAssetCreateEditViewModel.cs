namespace ITHelpDesk.ViewModels.Assets
{
    public class VirtualAssetCreateEditViewModel : ComputerCreateEditViewModel
    {
        // Virtual Machine / Virtual Host specific fields
        public string VMPlatform { get; set; } // "HyperV", "VMware", "Others"
        public int? AllocatedMemoryGB { get; set; }
        public int? AllocatedCPUCores { get; set; }
        public string StorageLocation { get; set; }
        public int? VirtualHostId { get; set; } // For VirtualMachine to link to its host
        public string HostName { get; set; } // Display the host name
    }
}
