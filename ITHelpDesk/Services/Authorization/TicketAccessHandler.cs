using System.Security.Claims;
using System.Threading.Tasks;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Authorization;

namespace ITHelpDesk.Services.Authorization;

public sealed class TicketAccessHandler : AuthorizationHandler<TicketAccessRequirement, Ticket>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TicketAccessRequirement requirement,
        Ticket resource)
    {
        if (resource == null)
        {
            return Task.CompletedTask;
        }

        if (context.User.IsInRole("Admin") || context.User.IsInRole("Support"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null &&
            (resource.CreatedById == userId || resource.AssignedToId == userId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

