using System;
using System.Collections.Generic;

namespace ITHelpDesk.ViewModels;

public class AdminUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? CreatedAt { get; set; }
}

