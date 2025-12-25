using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.ViewModels;

public class AccessRequestCreateViewModel
{
    // Applicant Information
    [Required]
    [MaxLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Display(Name = "Employee Number")]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Department")]
    public string Department { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required]
    [Display(Name = "Direct Manager")]
    public string SelectedManagerId { get; set; } = string.Empty;

    // Access Request Details
    [Required]
    [Display(Name = "Access Type")]
    public string AccessType { get; set; } = string.Empty; // Will be "Full Access", "Read Only", etc. - converted to enum

    [Required]
    [MaxLength(150)]
    [Display(Name = "System Name")]
    public string SystemName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    [Display(Name = "Reason for Access")]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Access Duration")]
    public string? AccessDuration { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Access Start Date")]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Access End Date")]
    public DateTime? EndDate { get; set; }

    // Attachments
    [Display(Name = "Attachments")]
    public List<IFormFile>? Attachments { get; set; }

    // Notes
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    // For dropdown population
    public IEnumerable<string>? Departments { get; set; }
    public IEnumerable<UserLookupViewModel>? Managers { get; set; }
}

