using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// نموذج المحطة الطرفية (Workstation)
/// </summary>
public class Workstation : Asset
{
    // Computer Info
    public int? ComputerInfoId { get; set; }

    [ForeignKey(nameof(ComputerInfoId))]
    public ComputerInfo? ComputerInfo { get; set; }

    // Operating System Info
    public int? OperatingSystemInfoId { get; set; }

    [ForeignKey(nameof(OperatingSystemInfoId))]
    public OperatingSystemInfo? OperatingSystemInfo { get; set; }

    // Memory Details
    public int? MemoryDetailsId { get; set; }

    [ForeignKey(nameof(MemoryDetailsId))]
    public MemoryDetails? MemoryDetails { get; set; }

    // Processor
    public int? ProcessorId { get; set; }

    [ForeignKey(nameof(ProcessorId))]
    public Processor? Processor { get; set; }

    // Hard Disk
    public int? HardDiskId { get; set; }

    [ForeignKey(nameof(HardDiskId))]
    public HardDisk? HardDisk { get; set; }

    // Keyboard
    public int? KeyboardId { get; set; }

    [ForeignKey(nameof(KeyboardId))]
    public Keyboard? Keyboard { get; set; }

    // Mouse
    public int? MouseId { get; set; }

    [ForeignKey(nameof(MouseId))]
    public Mouse? Mouse { get; set; }

    // Monitor
    public int? MonitorId { get; set; }

    [ForeignKey(nameof(MonitorId))]
    public Monitor? Monitor { get; set; }
}
