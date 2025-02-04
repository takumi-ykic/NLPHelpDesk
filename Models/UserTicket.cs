namespace NLPHelpDesk.Models;

/// <summary>
/// Represents the association between a user and a ticket.
/// </summary>
public class UserTicket
{
    public string UserId { get; set; }
    public string TicketId { get; set; }
    
    public AppUser AppUser { get; set; }
    public Ticket Ticket { get; set; }
}