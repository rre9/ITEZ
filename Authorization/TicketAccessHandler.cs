using System.Security.Claims;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Authorization;

public class TicketAccessHandler : AuthorizationHandler<TicketAccessRequirement>
{
    private readonly ApplicationDbContext _context;

    public TicketAccessHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TicketAccessRequirement requirement)
    {
        if (context.User is null)
        {
            return;
        }

        if (context.User.IsInRole("Admin") || context.User.IsInRole("Support"))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is not AuthorizationFilterContext mvcContext)
        {
            return;
        }

        if (!mvcContext.RouteData.Values.TryGetValue("id", out var idValue) || idValue is null)
        {
            return;
        }

        if (!int.TryParse(idValue.ToString(), out var ticketId))
        {
            return;
        }

        var userId = context.User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return;
        }

        var ticket = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => new { t.CreatedById, t.AssignedToId })
            .FirstOrDefaultAsync();

        if (ticket is null)
        {
            context.Succeed(requirement);
            return;
        }

        if (ticket.CreatedById == userId || ticket.AssignedToId == userId)
        {
            context.Succeed(requirement);
        }
    }
}
