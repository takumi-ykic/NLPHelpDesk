namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents the view model for displaying ticket details.
/// </summary>
public class TicketDetailsViewModel
{
    public Ticket Ticket { get; set; }
    public bool IsOwner { get; set; } = false;
    public bool IsAssigned { get; set; } = false;
}