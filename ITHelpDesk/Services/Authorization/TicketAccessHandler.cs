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

        // Allow if user is the selected manager for an Access Request or Service Request associated with this ticket
        // This works for both pending and approved states (Manager can always view their assigned requests)
        var isSelectedManagerForAccess = await _context.AccessRequests
            .AnyAsync(ar => ar.TicketId == resource.Id && ar.SelectedManagerId == userId);

        var isSelectedManagerForService = await _context.ServiceRequests
            .AnyAsync(sr => sr.TicketId == resource.Id && sr.SelectedManagerId == userId);

        if (isSelectedManagerForAccess || isSelectedManagerForService)
        {
            context.Succeed(requirement);
            return;
        }

        // Allow if user is Manager who has approved/rejected an Access Request or Service Request for this ticket
        // This ensures Manager can always view tickets they've reviewed, even after reassignment to Security
        if (context.User.IsInRole("Manager"))
        {
            var hasManagerReviewedAccess = await _context.AccessRequests
                .AnyAsync(ar => ar.TicketId == resource.Id &&
                               ar.SelectedManagerId == userId &&
                               ar.ManagerApprovalStatus != Models.ApprovalStatus.Pending);

            var hasManagerReviewedService = await _context.ServiceRequests
                .AnyAsync(sr => sr.TicketId == resource.Id &&
                               sr.SelectedManagerId == userId &&
                               sr.ManagerApprovalStatus != Models.ApprovalStatus.Pending);

            if (hasManagerReviewedAccess || hasManagerReviewedService)
            {
                context.Succeed(requirement);
                return;
            }
        }

        // Allow if user is Security who has approved/rejected an Access Request or Service Request for this ticket
        // This ensures Security can always view tickets they've reviewed, even after reassignment to IT
        // Note: Security viewing tickets pending their review is already handled by the assignee check above
        if (context.User.IsInRole("Security"))
        {
            var hasSecurityReviewedAccess = await _context.AccessRequests
                .AnyAsync(ar => ar.TicketId == resource.Id &&
                               ar.SecurityApprovalStatus != Models.ApprovalStatus.Pending);

            var hasSecurityReviewedService = await _context.ServiceRequests
                .AnyAsync(sr => sr.TicketId == resource.Id &&
                               sr.SecurityApprovalStatus != Models.ApprovalStatus.Pending);

            if (hasSecurityReviewedAccess || hasSecurityReviewedService)
            {
                context.Succeed(requirement);
                return;
            }
        }

        // Allow if user is IT and this is an Access/Service Request in IT stage
        // IT can view tickets that are in IT stage (Security approved, Status InProgress)
        if (context.User.IsInRole("IT"))
        {
            var isAccessRequestInITStage = await _context.AccessRequests
                .AnyAsync(ar => ar.TicketId == resource.Id &&
                               ar.SecurityApprovalStatus == Models.ApprovalStatus.Approved &&
                               resource.Status == Models.TicketStatus.InProgress);

            var isServiceRequestInITStage = await _context.ServiceRequests
                .AnyAsync(sr => sr.TicketId == resource.Id &&
                               sr.SecurityApprovalStatus == Models.ApprovalStatus.Approved &&
                               resource.Status == Models.TicketStatus.InProgress);

            if (isAccessRequestInITStage || isServiceRequestInITStage)
            {
                context.Succeed(requirement);
                return;
            }
        }

        // Allow Manager, Security, and IT to view System Change Requests
        // - Manager: Can view if their ManagerId is in the description
        // - Security: Can view all System Change Requests (Department=Security)
        // - IT: Can view System Change Requests assigned to them or approved by Security
        if (resource.Title != null && resource.Title.StartsWith("System Change Request", StringComparison.OrdinalIgnoreCase))
        {
            if (context.User.IsInRole("Manager"))
            {
                // Manager can view if their ID is in the description (they were involved in approval)
                if (resource.Description != null && resource.Description.Contains($"ManagerId={userId}"))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            if (context.User.IsInRole("Security"))
            {
                // Security can view all System Change Requests with Security department
                if (resource.Department == "Security")
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            if (context.User.IsInRole("IT"))
            {
                // IT can view System Change Requests assigned to them or after Security approval
                var isApproved = resource.Description != null &&
                                resource.Description.Contains("ManagerApprovalStatus=Approved", StringComparison.OrdinalIgnoreCase);
                if (isApproved || resource.AssignedToId == userId)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}

