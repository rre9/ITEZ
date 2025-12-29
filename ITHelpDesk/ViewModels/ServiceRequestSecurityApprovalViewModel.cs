using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.ViewModels;

public class ServiceRequestSecurityApprovalViewModel
{
    public int TicketId { get; set; }
    
    public Ticket Ticket { get; set; } = default!;
    
    public ServiceRequest ServiceRequest { get; set; } = default!;
    
    public bool IsAuthorizedSecurity { get; set; }
    
    public bool IsReadOnly { get; set; }
    
    public bool CanApprove { get; set; }
    
    [MaxLength(1000)]
    [Display(Name = "Comment")]
    public string? Comment { get; set; }
    
    [Display(Name = "Attachments (Images/PDFs)")]
    public List<IFormFile>? Attachments { get; set; }
    
    public IEnumerable<TicketLog> Logs { get; set; } = new List<TicketLog>();
}

