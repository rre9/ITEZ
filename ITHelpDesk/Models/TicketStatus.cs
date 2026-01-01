namespace ITHelpDesk.Models;

public enum TicketStatus
{
    New = 0,
    InProgress = 1,
    Resolved = 2,
    Rejected = 3,
    Closed = 4
}

public enum CloseReason
{
    Completed = 0,
    Rejected = 1
}

