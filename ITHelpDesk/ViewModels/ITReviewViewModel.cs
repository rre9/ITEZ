using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.ViewModels;

/// <summary>
/// ViewModel for IT Review page - Final decision point for IT users.
/// Supports both AccessRequest and ServiceRequest types.
/// </summary>
public class ITReviewViewModel
{
    public int TicketId { get; set; }
    
    public Ticket Ticket { get; set; } = default!;
    
    public AccessRequest? AccessRequest { get; set; }
    
    public ServiceRequest? ServiceRequest { get; set; }
    
    public bool IsAuthorizedIT { get; set; }
    
    public bool IsReadOnly { get; set; }
    
    public bool CanReview { get; set; }
    
    [Required(ErrorMessage = "Comment is required")]
    [MaxLength(1000)]
    [Display(Name = "Comment")]
    public string? Comment { get; set; }
    
    [Display(Name = "Attachments (Images/PDFs)")]
    public List<IFormFile>? Attachments { get; set; }
    
    public IEnumerable<TicketLog> Logs { get; set; } = new List<TicketLog>();
}

