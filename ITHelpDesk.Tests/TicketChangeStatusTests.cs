using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ITHelpDesk.Tests;

public class TicketChangeStatusTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TicketChangeStatusTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_ChangeStatus_AddsLogEntry()
    {
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var admin = await userManager.FindByEmailAsync("admin@yub.com.sa");
        var employee = await userManager.FindByEmailAsync("employee@yub.com.sa");

        var ticket = new Ticket
        {
            Title = "Email outage",
            Description = "Unable to send emails",
            Department = "IT Operations",
            Priority = TicketPriority.Medium,
            Status = TicketStatus.New,
            CreatedById = employee!.Id,
            CreatedAt = DateTime.UtcNow
        };

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        db.Entry(ticket).State = EntityState.Detached;

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
        await client.LoginAsync("admin@yub.com.sa", "Admin#12345!");

        var changeGet = await client.GetAsync($"/Tickets/ChangeStatus/{ticket.Id}");
        changeGet.EnsureSuccessStatusCode();
        var token = await TestHelpers.ExtractAntiForgeryTokenAsync(changeGet);

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
            new KeyValuePair<string, string>("TicketId", ticket.Id.ToString()),
            new KeyValuePair<string, string>("NewStatus", TicketStatus.Resolved.ToString()),
            new KeyValuePair<string, string>("CurrentStatus", ticket.Status.ToString()),
            new KeyValuePair<string, string>("InternalNotes", "Issue resolved"),
            new KeyValuePair<string, string>("AssignedToId", string.Empty)
        });

        var response = await client.PostAsync($"/Tickets/ChangeStatus/{ticket.Id}", form);
        response.EnsureSuccessStatusCode();

        var updatedTicket = await db.Tickets.Include(t => t.Logs).FirstAsync(t => t.Id == ticket.Id);
        var statusLog = updatedTicket.Logs.OrderByDescending(l => l.Timestamp).FirstOrDefault();

        Assert.NotNull(statusLog);
        Assert.Equal("Status Update", statusLog!.Action);
        Assert.Equal(TicketStatus.Resolved, updatedTicket.Status);
    }
}
