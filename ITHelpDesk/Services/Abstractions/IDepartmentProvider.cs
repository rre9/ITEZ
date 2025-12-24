using System.Collections.Generic;

namespace ITHelpDesk.Services.Abstractions;

public interface IDepartmentProvider
{
    IReadOnlyList<string> GetDepartments();
}

