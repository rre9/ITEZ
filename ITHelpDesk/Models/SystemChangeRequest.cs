using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Models;

public class SystemChangeRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TicketId { get; set; }

    [ForeignKey(nameof(TicketId))]
    public Ticket Ticket { get; set; } = default!;

    // Requester Information
    [Required]
    [MaxLength(150)]
    public string RequesterName { get; set; } = default!;

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    // Change Details
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = default!;

    [Required]
    [MaxLength(2000)]
    public string ChangeDescription { get; set; } = default!;

    [Required]
    [MaxLength(2000)]
    public string ChangeReason { get; set; } = default!;

    [MaxLength(100)]
    public string? ChangeType { get; set; }

    [MaxLength(50)]
    public string? ChangePriority { get; set; }

    [MaxLength(50)]
    public string? ChangeImpact { get; set; }

    [MaxLength(1000)]
    public string? AffectedAssets { get; set; }

    // Implementation Details
    [Required]
    [MaxLength(2000)]
    public string ImplementationPlan { get; set; } = default!;

    [Required]
    [MaxLength(2000)]
    public string BackoutPlan { get; set; } = default!;

    [MaxLength(150)]
    public string? ImplementerName { get; set; }

    public DateTime? ExecutionDate { get; set; }

    // Approval Status
    public ApprovalStatus ManagerApprovalStatus { get; set; } = ApprovalStatus.Pending;

    public string? ManagerApprovalComment { get; set; }

    public DateTime? ManagerApprovedAt { get; set; }

    public ApprovalStatus SecurityApprovalStatus { get; set; } = ApprovalStatus.Pending;

    public string? SecurityApprovalComment { get; set; }

    public DateTime? SecurityApprovedAt { get; set; }

    // Selected Manager
    [Required]
    [MaxLength(450)]
    public string SelectedManagerId { get; set; } = default!;

    [ForeignKey(nameof(SelectedManagerId))]
    public ApplicationUser SelectedManager { get; set; } = default!;

    // Timestamps
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
