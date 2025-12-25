using System.Collections.Generic;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels;

public class ITDashboardViewModel
{
    public IEnumerable<AccessRequest> PendingRequests { get; set; } = new List<AccessRequest>();
}

