using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.ViewModels;

public class AccessRequestExecutionViewModel
{
    public int TicketId { get; set; }
    
    public Ticket Ticket { get; set; } = default!;
    
    public AccessRequest AccessRequest { get; set; } = default!;
    
    public bool IsAuthorizedIT { get; set; }
    
    public bool IsReadOnly { get; set; }
    
    public bool CanExecute { get; set; }
    
    [MaxLength(1000)]
    [Display(Name = "Execution Notes")]
    public string? ExecutionNotes { get; set; }
    
    [MaxLength(1000)]
    [Display(Name = "Closure Reason")]
    public string? ClosureReason { get; set; }
    
    [Display(Name = "Attachments (Images/PDFs)")]
    public List<IFormFile>? Attachments { get; set; }
    
    public IEnumerable<TicketLog> Logs { get; set; } = new List<TicketLog>();
}

