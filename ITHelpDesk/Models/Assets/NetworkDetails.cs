using System;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models.Assets;

/// <summary>
/// معلومات الشبكة للأجهزة
/// </summary>
public class NetworkDetails
{
    [Key]
    public int Id { get; set; }

    [MaxLength(15)]
    public string? IPAddress { get; set; }

    [MaxLength(17)]
    public string? MACAddress { get; set; }

    [MaxLength(50)]
    public string? NIC { get; set; }

    [MaxLength(100)]
    public string? Network { get; set; }

    [MaxLength(15)]
    public string? DefaultGateway { get; set; }

    public bool DHCPEnabled { get; set; } = false;

    [MaxLength(15)]
    public string? DHCPServer { get; set; }

    [MaxLength(100)]
    public string? DNSHostname { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
