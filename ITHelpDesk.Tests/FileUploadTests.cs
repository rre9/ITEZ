using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ITHelpDesk.Tests;

public class FileUploadTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public FileUploadTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTicket_Rejects_InvalidFileExtension()
    {
        var client = _factory.CreateClient();
        await client.LoginAsync("employee@yub.com.sa", "Admin#12345!");

        var createGet = await client.GetAsync("/Tickets/Create");
        createGet.EnsureSuccessStatusCode();
        var token = await TestHelpers.ExtractAntiForgeryTokenAsync(createGet);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(token), "__RequestVerificationToken");
        form.Add(new StringContent("Printer issue"), "Title");
        form.Add(new StringContent("The main printer is jammed."), "Description");
        form.Add(new StringContent("IT Operations"), "Department");
        form.Add(new StringContent("High"), "Priority");

        var invalidFile = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("fake"));
        invalidFile.Headers.Add("Content-Disposition", "form-data; name=\"Attachments\"; filename=\"evidence.exe\"");
        invalidFile.Headers.Add("Content-Type", "application/octet-stream");
        form.Add(invalidFile, "Attachments", "evidence.exe");

        var response = await client.PostAsync("/Tickets/Create", form);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Unsupported file type", body);
    }
}
