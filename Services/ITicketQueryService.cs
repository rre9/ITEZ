using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITHelpDesk.Models;
using ITHelpDesk.ViewModels;

namespace ITHelpDesk.Services;

public interface ITicketQueryService
{
    Task<TicketQueryResult> GetTicketsAsync(TicketsQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetTicketsForExportAsync(TicketsQuery query, CancellationToken cancellationToken = default);
}

