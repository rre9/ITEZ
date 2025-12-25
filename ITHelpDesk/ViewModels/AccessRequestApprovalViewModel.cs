using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels;

public class AccessRequestApprovalViewModel
{
    public int TicketId { get; set; }
    
    public Ticket Ticket { get; set; } = default!;
    
    public AccessRequest AccessRequest { get; set; } = default!;
    
    public bool IsAuthorizedManager { get; set; }
    
    public bool IsReadOnly { get; set; }
    
    [MaxLength(1000)]
    [Display(Name = "Comment")]
    public string? Comment { get; set; }
    
    public IEnumerable<TicketLog> Logs { get; set; } = new List<TicketLog>();
    
    public string SelectedManagerName { get; set; } = string.Empty;
}

