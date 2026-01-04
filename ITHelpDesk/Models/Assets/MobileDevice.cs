using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// نموذج الجهاز المحمول العام (Mobile Device)
/// </summary>
public class MobileDevice : Asset
{
    // Mobile Details
    public int? MobileDetailsId { get; set; }

    [ForeignKey(nameof(MobileDetailsId))]
    public MobileDetails? MobileDetails { get; set; }

    // Operating System Info
    public int? OperatingSystemInfoId { get; set; }

    [ForeignKey(nameof(OperatingSystemInfoId))]
    public OperatingSystemInfo? OperatingSystemInfo { get; set; }
}
