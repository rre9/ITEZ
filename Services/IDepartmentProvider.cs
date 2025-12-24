using System.Collections.Generic;

namespace ITHelpDesk.Services;

public interface IDepartmentProvider
{
    IReadOnlyList<string> GetDepartments();
}

