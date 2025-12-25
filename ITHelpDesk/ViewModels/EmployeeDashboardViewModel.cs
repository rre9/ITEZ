using System.Collections.Generic;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels;

public class EmployeeDashboardViewModel
{
    public IEnumerable<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
    public string SelectedStatus { get; set; } = "All";
}

