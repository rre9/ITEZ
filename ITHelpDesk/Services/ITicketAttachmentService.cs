using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.Services;

public interface ITicketAttachmentService
{
    Task<TicketAttachmentMetadata> SaveAttachmentAsync(int ticketId, IFormFile file, CancellationToken cancellationToken = default);
}

