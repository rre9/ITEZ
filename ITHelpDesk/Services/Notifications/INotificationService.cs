using System.Threading.Tasks;
using ITHelpDesk.Models;

namespace ITHelpDesk.Services.Notifications;

/// <summary>
/// Service interface for sending notifications related to Access Request workflow.
/// This abstraction allows for easy future integration with Microsoft Graph or other email providers.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notifies the selected Direct Manager that an Access Request requires their approval.
    /// </summary>
    /// <param name="accessRequest">The Access Request that requires manager approval</param>
    Task NotifyManagerAsync(AccessRequest accessRequest);

    /// <summary>
    /// Notifies Security (Mohammed) that an Access Request requires Security approval.
    /// </summary>
    /// <param name="accessRequest">The Access Request that requires security approval</param>
    Task NotifySecurityAsync(AccessRequest accessRequest);

    /// <summary>
    /// Notifies IT (Yazan) that an Access Request requires execution.
    /// </summary>
    /// <param name="accessRequest">The Access Request that requires IT execution</param>
    Task NotifyITAsync(AccessRequest accessRequest);

    /// <summary>
    /// Notifies the employee, manager, and security when an Access Request is completed or closed by IT.
    /// </summary>
    /// <param name="accessRequest">The Access Request that was completed or closed</param>
    Task NotifyRequestCompletedAsync(AccessRequest accessRequest);
}

