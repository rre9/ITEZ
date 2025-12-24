using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.ViewModels;

public class TicketCreateViewModel
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public List<IFormFile>? Attachments { get; set; }

    public IEnumerable<TicketPriority> AvailablePriorities { get; set; } = new List<TicketPriority>();

    public IEnumerable<TicketStatus> AvailableStatuses { get; set; } = new List<TicketStatus>();

    public IEnumerable<string>? Departments { get; set; }

    [Display(Name = "Assign To")]
    public string? AssignedToId { get; set; }

    public IEnumerable<UserLookupViewModel>? SupportUsers { get; set; }

    public bool CanAssign { get; set; } = false; // Whether user can assign tickets to others
}

