using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpDesk.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route("Error/{statusCode:int}")]
    public IActionResult Status(int statusCode)
    {
        return statusCode switch
        {
            403 => View("403"),
            404 => View("404"),
            _ => View("404")
        };
    }

    [Route("Error/500")]
    public IActionResult Error()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        ViewData["ErrorMessage"] = exceptionFeature?.Error.Message;
        return View("500");
    }
}
