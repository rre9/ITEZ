using ITHelpDesk.Models.Assets;

namespace ITHelpDesk.ViewModels.Assets
{
    public class ComputerCreateEditViewModel : AssetCreateEditViewModel
    {
        // Computer-specific fields
        public int? ComputerInfoId { get; set; }

        // Operating System
        public string OSName { get; set; }
        public string OSVersion { get; set; }
        public string OSArchitecture { get; set; }

        // BIOS
        public string BIOSVersion { get; set; }
        public string BIOSManufacturer { get; set; }
        public DateTime? BIOSReleaseDate { get; set; }

        // Memory Details
        public int? TotalMemoryGB { get; set; }
        public string MemoryType { get; set; }
        public int? MemorySlots { get; set; }

        // Processor
        public string ProcessorModel { get; set; }
        public int? ProcessorCores { get; set; }
        public decimal? ProcessorClockSpeedGHz { get; set; }

        // Hard Disk
        public string HardDiskModel { get; set; }
        public int? HardDiskCapacityGB { get; set; }
        public string HardDiskType { get; set; } // SSD, HDD, NVMe

        // Peripherals
        public string KeyboardModel { get; set; }
        public string KeyboardType { get; set; }

        public string MouseModel { get; set; }
        public string MouseType { get; set; }

        public string MonitorModel { get; set; }
        public string MonitorSize { get; set; }
        public string MonitorResolution { get; set; }
    }
}
