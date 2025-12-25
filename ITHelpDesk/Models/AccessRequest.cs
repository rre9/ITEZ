using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Models;

public class AccessRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TicketId { get; set; }

    [ForeignKey(nameof(TicketId))]
    public Ticket Ticket { get; set; } = default!;

    // Applicant Information
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = default!;

    [Required]
    [MaxLength(50)]
    public string EmployeeNumber { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    // Access Request Details
    [Required]
    public AccessType AccessType { get; set; }

    [Required]
    [MaxLength(150)]
    public string SystemName { get; set; } = default!;

    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = default!;

    [MaxLength(100)]
    public string? AccessDuration { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    // Selected Manager
    [Required]
    [MaxLength(450)]
    public string SelectedManagerId { get; set; } = default!;

    [ForeignKey(nameof(SelectedManagerId))]
    public ApplicationUser SelectedManager { get; set; } = default!;

    // Manager Approval
    [MaxLength(150)]
    public string? ManagerApprovalName { get; set; }

    public DateTime? ManagerApprovalDate { get; set; }

    public ApprovalStatus ManagerApprovalStatus { get; set; } = ApprovalStatus.Pending;

    // IT Department Approval
    [MaxLength(150)]
    public string? ITApprovalName { get; set; }

    public DateTime? ITApprovalDate { get; set; }

    public ApprovalStatus ITApprovalStatus { get; set; } = ApprovalStatus.Pending;

    // Security Manager Approval
    [MaxLength(150)]
    public string? SecurityApprovalName { get; set; }

    public DateTime? SecurityApprovalDate { get; set; }

    public ApprovalStatus SecurityApprovalStatus { get; set; } = ApprovalStatus.Pending;

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

