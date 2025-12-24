using System.Collections.Generic;
using System.Linq;
using ITHelpDesk.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace ITHelpDesk.Services;

public class DepartmentProvider : IDepartmentProvider
{
    private readonly IReadOnlyList<string> _departments;

    public DepartmentProvider(IOptions<DepartmentOptions> options)
    {
        var configured = options.Value.Items
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d.Trim())
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        _departments = configured.Count > 0
            ? configured
            : new List<string> { "IT Operations", "Networking", "Security", "Infrastructure", "Applications" };
    }

    public IReadOnlyList<string> GetDepartments() => _departments;
}

