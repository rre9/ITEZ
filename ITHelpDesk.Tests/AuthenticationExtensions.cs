using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ITHelpDesk.Tests;

public static class AuthenticationExtensions
{
    public static async Task LoginAsync(this HttpClient client, string email, string password)
    {
        var loginGet = await client.GetAsync("/Identity/Account/Login");
        loginGet.EnsureSuccessStatusCode();
        var antiForgeryToken = await TestHelpers.ExtractAntiForgeryTokenAsync(loginGet);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Input.Email", email),
            new KeyValuePair<string, string>("Input.Password", password),
            new KeyValuePair<string, string>("Input.RememberMe", "false"),
            new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken)
        });

        var response = await client.PostAsync("/Identity/Account/Login?ReturnUrl=%2F", content);
        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Found)
        {
            response.EnsureSuccessStatusCode();
        }
    }
}
