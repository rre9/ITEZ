using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace ITHelpDesk.Tests;

public static class TestHelpers
{
    public static async Task<string> ExtractAntiForgeryTokenAsync(HttpResponseMessage response)
    {
        var html = await response.Content.ReadAsStringAsync();
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html);
        var tokenElement = document.QuerySelector("input[name='__RequestVerificationToken']");
        if (tokenElement == null)
        {
            throw new InvalidOperationException("Antiforgery token not found in HTML response.");
        }

        return tokenElement.GetAttribute("value") ?? string.Empty;
    }
}
