using System.Security.Claims;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Services.Authorization;

public sealed class TicketAccessHandler : AuthorizationHandler<TicketAccessRequirement, Ticket>
{
    private readonly ApplicationDbContext _context;

    public TicketAccessHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TicketAccessRequirement requirement,
        Ticket resource)
    {
        if (resource == null)
        {
            return;
        }

        if (context.User.IsInRole("Admin") || context.User.IsInRole("Support"))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return;
        }

        // Allow if user is creator or assigned to the ticket
        if (resource.CreatedById == userId || resource.AssignedToId == userId)
        {
            context.Succeed(requirement);
            return;
        }

        // Allow if user is the selected manager for an Access Request associated with this ticket
        var isSelectedManager = await _context.AccessRequests
            .AnyAsync(ar => ar.TicketId == resource.Id && ar.SelectedManagerId == userId);
        
        if (isSelectedManager)
        {
            context.Succeed(requirement);
        }
    }
}

