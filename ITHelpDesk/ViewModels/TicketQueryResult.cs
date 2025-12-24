using System.Collections.Generic;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels;

public class TicketQueryResult
{
    public IReadOnlyList<Ticket> Items { get; init; } = new List<Ticket>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public TicketsQuery Query { get; init; } = new();
}

