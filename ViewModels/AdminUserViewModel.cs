using System;
using System.Collections.Generic;

namespace ITHelpDesk.ViewModels;

public class AdminUserViewModel
{
    public string Id { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public bool IsLockedOut { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public bool EmailConfirmed { get; init; }
    public DateTime? CreatedAt { get; init; }
}

