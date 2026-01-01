using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// معلومات الذاكرة
/// </summary>
public class MemoryDetails
{
    [Key]
    public int Id { get; set; }

    public int? RAM { get; set; } // في GB

    public int? VirtualMemory { get; set; } // في GB

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
