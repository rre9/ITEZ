using System.Collections.Generic;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels;

public class SecurityDashboardViewModel
{
    public IEnumerable<AccessRequest> PendingRequests { get; set; } = new List<AccessRequest>();
    public IEnumerable<ServiceRequest> PendingServiceRequests { get; set; } = new List<ServiceRequest>();
}

