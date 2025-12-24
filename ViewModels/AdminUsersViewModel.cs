using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITHelpDesk.ViewModels;

public class AdminUsersViewModel
{
    public IReadOnlyList<AdminUserViewModel> Users { get; init; } = new List<AdminUserViewModel>();
    public string? Search { get; init; }
    public string? Role { get; init; }
    public IReadOnlyList<SelectListItem> RoleOptions { get; init; } = new List<SelectListItem>();
}

