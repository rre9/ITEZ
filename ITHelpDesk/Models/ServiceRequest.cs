using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Models;

public class ServiceRequest
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
    public string EmployeeName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string JobTitle { get; set; } = default!;

    [Required]
    public DateTime RequestDate { get; set; }

    // Service Request Details
    [Required]
    [MaxLength(2000)]
    public string UsageDescription { get; set; } = default!;

    [Required]
    [MaxLength(2000)]
    public string UsageReason { get; set; } = default!;

    // Acknowledgment & Signature
    [Required]
    public bool Acknowledged { get; set; }

    [Required]
    [MaxLength(150)]
    public string SignatureName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string SignatureJobTitle { get; set; } = default!;

    [Required]
    public DateTime SignatureDate { get; set; }

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

