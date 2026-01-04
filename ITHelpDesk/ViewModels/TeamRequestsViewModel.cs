using System;
using System.Collections.Generic;
using System.Linq;
using ITHelpDesk.Models;

namespace ITHelpDesk.ViewModels
{
    public class TeamRequestsViewModel
    {
        public List<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
        public List<Ticket> SystemChangeTickets { get; set; } = new List<Ticket>();
        public List<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        // Combined and sorted requests
        public IEnumerable<object> AllRequests
        {
            get
            {
                var combined = new List<(DateTime Date, object Item)>();

                foreach (var ar in AccessRequests)
                {
                    combined.Add((ar.CreatedAt, ar));
                }

                foreach (var ticket in SystemChangeTickets)
                {
                    combined.Add((ticket.CreatedAt, ticket));
                }

                foreach (var sr in ServiceRequests)
                {
                    combined.Add((sr.CreatedAt, sr));
                }

                return combined.OrderByDescending(x => x.Date).Select(x => x.Item);
            }
        }
    }
}
