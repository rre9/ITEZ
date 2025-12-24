using System;
using System.Collections.Generic;
using System.Linq;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels;

public class TicketsQuery
{
    private const int DefaultPageSize = 10;
    private static readonly int[] AllowedPageSizes = { 10, 20, 50 };

    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? Department { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Requester { get; set; }
    public string? Assignee { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;

    public void Normalize()
    {
        if (Page < 1)
        {
            Page = 1;
        }

        if (!AllowedPageSizes.Contains(PageSize))
        {
            PageSize = DefaultPageSize;
        }

        SortDirection = SortDirection?.Equals("asc", StringComparison.OrdinalIgnoreCase) == true
            ? "asc"
            : "desc";

        if (string.IsNullOrWhiteSpace(SortBy))
        {
            SortBy = "createdAt";
        }
    }

    public Dictionary<string, object?> ToRouteValues(bool includePagination = true)
    {
        var values = new Dictionary<string, object?>();

        if (Status is not null)
        {
            values[nameof(Status)] = Status;
        }

        if (Priority is not null)
        {
            values[nameof(Priority)] = Priority;
        }

        if (!string.IsNullOrWhiteSpace(Department))
        {
            values[nameof(Department)] = Department;
        }

        if (From is not null)
        {
            values[nameof(From)] = From.Value.ToString("yyyy-MM-dd");
        }

        if (To is not null)
        {
            values[nameof(To)] = To.Value.ToString("yyyy-MM-dd");
        }

        if (!string.IsNullOrWhiteSpace(Requester))
        {
            values[nameof(Requester)] = Requester;
        }

        if (!string.IsNullOrWhiteSpace(Assignee))
        {
            values[nameof(Assignee)] = Assignee;
        }

        if (!string.IsNullOrWhiteSpace(SortBy))
        {
            values[nameof(SortBy)] = SortBy;
        }

        if (!string.IsNullOrWhiteSpace(SortDirection))
        {
            values[nameof(SortDirection)] = SortDirection;
        }

        if (includePagination)
        {
            values[nameof(Page)] = Page;
            values[nameof(PageSize)] = PageSize;
        }

        return values;
    }
}

