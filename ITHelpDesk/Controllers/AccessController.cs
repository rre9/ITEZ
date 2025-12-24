using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpDesk.Controllers;

[AllowAnonymous]
public class AccessController : Controller
{
    [Route("AccessDenied")]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(returnUrl) &&
            returnUrl.StartsWith("/Tickets/Create", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Toast"] = "⚠️ Only employees can submit new tickets.";
            return RedirectToAction("Index", "Tickets");
        }

        Response.StatusCode = 403;
        return View("~/Views/Error/403.cshtml");
    }
}

