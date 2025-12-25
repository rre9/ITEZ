using System.Collections.Generic;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels;

public class TasksViewModel
{
    public IEnumerable<Ticket> Tickets { get; set; } = new List<Ticket>();
    public Dictionary<int, TicketReviewInfo> ReviewInfo { get; set; } = new Dictionary<int, TicketReviewInfo>();
}

public class TicketReviewInfo
{
    public bool CanReview { get; set; }
    public string? ReviewAction { get; set; }
}
