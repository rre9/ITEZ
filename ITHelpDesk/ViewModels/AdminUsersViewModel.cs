using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITHelpDesk.ViewModels;

public class AdminUsersViewModel
{
    public List<AdminUserViewModel> Users { get; set; } = new();
    public string? Search { get; set; }
    public string? Role { get; set; }
    public List<SelectListItem> RoleOptions { get; set; } = new();
}

