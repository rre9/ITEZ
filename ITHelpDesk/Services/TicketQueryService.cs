using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.Services.Abstractions;
using ITHelpDesk.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Services;

public class TicketQueryService : ITicketQueryService
{
    private readonly ApplicationDbContext _context;

    public TicketQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TicketQueryResult> GetTicketsAsync(TicketsQuery query, CancellationToken cancellationToken = default)
    {
        query.Normalize();

        var baseQuery = BuildQuery(query);

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        if (totalPages == 0)
        {
            totalPages = 1;
        }

        if (query.Page > totalPages)
        {
            query.Page = totalPages;
        }

        var items = await baseQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new TicketQueryResult
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Query = query
        };
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsForExportAsync(TicketsQuery query, CancellationToken cancellationToken = default)
    {
        query.Normalize();
        var baseQuery = BuildQuery(query);
        return await baseQuery.ToListAsync(cancellationToken);
    }

    private IQueryable<Ticket> BuildQuery(TicketsQuery query)
    {
        var q = _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (query.Status is not null)
        {
            q = q.Where(t => t.Status == query.Status.Value);
        }

        if (query.Priority is not null)
        {
            q = q.Where(t => t.Priority == query.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            q = q.Where(t => t.Department == query.Department);
        }

        if (query.From is not null)
        {
            var fromUtc = DateTime.SpecifyKind(query.From.Value.Date, DateTimeKind.Utc);
            q = q.Where(t => t.CreatedAt >= fromUtc);
        }

        if (query.To is not null)
        {
            var toExclusiveUtc = DateTime.SpecifyKind(query.To.Value.Date.AddDays(1), DateTimeKind.Utc);
            q = q.Where(t => t.CreatedAt < toExclusiveUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.Requester))
        {
            var term = query.Requester.Trim();
            q = q.Where(t =>
                t.CreatedBy != null && (
                    t.CreatedBy.FullName.Contains(term) ||
                    (t.CreatedBy.Email != null && t.CreatedBy.Email.Contains(term))
                ));
        }

        if (!string.IsNullOrWhiteSpace(query.Assignee))
        {
            var term = query.Assignee.Trim();
            q = q.Where(t =>
                t.AssignedTo != null && (
                    t.AssignedTo.FullName.Contains(term) ||
                    (t.AssignedTo.Email != null && t.AssignedTo.Email.Contains(term))
                ));
        }

        q = ApplySorting(q, query);

        return q;
    }

    private static IQueryable<Ticket> ApplySorting(IQueryable<Ticket> query, TicketsQuery filters)
    {
        var sortBy = filters.SortBy?.ToLowerInvariant();
        var sortDirection = filters.SortDirection == "asc" ? "asc" : "desc";

        return sortBy switch
        {
            "priority" => sortDirection == "asc"
                ? query.OrderBy(t => t.Priority).ThenByDescending(t => t.CreatedAt)
                : query.OrderByDescending(t => t.Priority).ThenByDescending(t => t.CreatedAt),
            "status" => sortDirection == "asc"
                ? query.OrderBy(t => t.Status).ThenByDescending(t => t.CreatedAt)
                : query.OrderByDescending(t => t.Status).ThenByDescending(t => t.CreatedAt),
            "department" => sortDirection == "asc"
                ? query.OrderBy(t => t.Department).ThenByDescending(t => t.CreatedAt)
                : query.OrderByDescending(t => t.Department).ThenByDescending(t => t.CreatedAt),
            "title" => sortDirection == "asc"
                ? query.OrderBy(t => t.Title).ThenByDescending(t => t.CreatedAt)
                : query.OrderByDescending(t => t.Title).ThenByDescending(t => t.CreatedAt),
            "createdat" or _ => sortDirection == "asc"
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt)
        };
    }
}

