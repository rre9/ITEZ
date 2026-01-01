using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.ViewModels;

public class ServiceRequestCreateViewModel
{
    // Applicant Information
    [Required]
    [MaxLength(150)]
    [Display(Name = "Employee Name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Department")]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Job Title")]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Request Date")]
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Display(Name = "Direct Manager")]
    public string SelectedManagerId { get; set; } = string.Empty;

    // Description of Requested Usage
    [Required]
    [MaxLength(2000)]
    [Display(Name = "Description of Requested Social Media Usage")]
    public string UsageDescription { get; set; } = string.Empty;

    // Business / Operational Reasons
    [Required]
    [MaxLength(2000)]
    [Display(Name = "Reason for Request")]
    public string UsageReason { get; set; } = string.Empty;

    // Acknowledgment & Signature
    [Required]
    [Display(Name = "I acknowledge and agree to the responsibility terms above")]
    public bool Acknowledged { get; set; }

    [Required]
    [MaxLength(150)]
    [Display(Name = "Signature Name")]
    public string SignatureName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Signature Job Title")]
    public string SignatureJobTitle { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Signature Date")]
    public DateTime SignatureDate { get; set; } = DateTime.UtcNow;

    // Attachments
    [Display(Name = "Attachments")]
    public List<IFormFile>? Attachments { get; set; }

    // For dropdown population
    public IEnumerable<string>? Departments { get; set; }
    public IEnumerable<UserLookupViewModel>? Managers { get; set; }
}

