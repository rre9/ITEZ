using ITHelpDesk.Models.Assets;

namespace ITHelpDesk.ViewModels.Assets
{
    public class ServerCreateEditViewModel : ComputerCreateEditViewModel
    {
        // Server-specific fields
        public string ServerRole { get; set; } // "Database", "Web", "FileServer", etc.
        public string OSEdition { get; set; } // "Standard", "DataCenter", etc.
        public bool? IsVirtualizationEnabled { get; set; }
        public int? NumberOfNetworkAdapters { get; set; }
    }
}
