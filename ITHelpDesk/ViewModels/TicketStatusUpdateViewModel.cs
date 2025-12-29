using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.ViewModels;

public class TicketStatusUpdateViewModel
{
    public int TicketId { get; set; }

    public TicketStatus CurrentStatus { get; set; }

    [Required]
    public TicketStatus NewStatus { get; set; }

    [Display(Name = "Assign To")]
    public string? AssignedToId { get; set; }

    [Display(Name = "Comment")]
    [MaxLength(1000)]
    public string? InternalNotes { get; set; }
    
    public bool RequireComment { get; set; } = false; // Whether comment is required (e.g., for IT final decision)

    [Display(Name = "Attachment (Image)")]
    public IFormFile? Attachment { get; set; }

    public IEnumerable<TicketStatus> AvailableStatuses { get; set; } = new List<TicketStatus>();

    public IEnumerable<UserLookupViewModel> SupportUsers { get; set; } = new List<UserLookupViewModel>();

    public bool CanAssign { get; set; } = true; // Whether user can assign tickets to others
}

