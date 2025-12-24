using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITHelpDesk.ViewModels;

public class TicketsIndexViewModel
{
    public TicketQueryResult Result { get; init; } = new();
    public TicketsQuery Query { get; init; } = new();
    public IReadOnlyList<SelectListItem> DepartmentOptions { get; init; } = new List<SelectListItem>();
    public IReadOnlyList<SelectListItem> StatusOptions { get; init; } = new List<SelectListItem>();
    public IReadOnlyList<SelectListItem> PriorityOptions { get; init; } = new List<SelectListItem>();
}

