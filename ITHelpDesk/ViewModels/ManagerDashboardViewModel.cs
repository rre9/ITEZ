using System.Collections.Generic;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels;

public class ManagerDashboardViewModel
{
    public IEnumerable<AccessRequest> PendingRequests { get; set; } = new List<AccessRequest>();
}

