using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ITHelpDesk.Tests;

public class TicketsAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TicketsAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Employee_Cannot_Access_AllTickets()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await client.LoginAsync("employee@yub.com.sa", "Admin#12345!");

        var response = await client.GetAsync("/Tickets/Index");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
