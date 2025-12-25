using System.Collections.Generic;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels;

public class AdminDashboardViewModel
{
    public IEnumerable<AccessRequest> AllRequests { get; set; } = new List<AccessRequest>();
    public int PendingManagerCount { get; set; }
    public int PendingSecurityCount { get; set; }
    public int PendingITCount { get; set; }
}

