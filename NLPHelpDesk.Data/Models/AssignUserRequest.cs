namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents a request to assign a user to a ticket.
/// </summary>
public class AssignUserRequest
{
    public string UserId { get; set; }
    public string TicketId { get; set; }
}