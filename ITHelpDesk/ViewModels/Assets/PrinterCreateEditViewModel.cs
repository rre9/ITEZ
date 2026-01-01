namespace ITHelpDesk.ViewModels.Assets
{
    public class PrinterCreateEditViewModel : AssetCreateEditViewModel
    {
        // Printer-specific fields
        public string PrinterType { get; set; } // "InkJet", "Laser", "DotMatrix"
        public string Model { get; set; }
        public int? PrinterPPM { get; set; } // Pages per minute
        public string ColorSupport { get; set; } // "Color", "BlackAndWhite"
        public bool? NetworkPrinter { get; set; }
        public string TonerCartridgeModel { get; set; }
        public DateTime? TonerExpiryDate { get; set; }
    }
}
