using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NLPHelpDesk.Models;

/// <summary>
/// Represents the completion details for a ticket.
/// </summary>
public class TicketCompletion
{
    public int CompletionId { get; set; }
    public string? TicketId { get; set; }
    public string? UserId { get; set; }
    [StringLength(400)]
    [DisplayName("Ticket Description")]
    public string? Question { get; set; }
    [StringLength(400)]
    [DisplayName("Solution")]
    public string? Answer { get; set; }
    public int? Category { get; set; }
    public string? Difficulty { get; set; }
    public int IsCsv { get; set; } = 0;
    public DateTime? CompletionDate { get; set; }
    
    public HelpDeskCategory? HelpDeskCategory { get; set; }
    public Ticket? Ticket { get; set; }
    public AppUser? AppUser { get; set; }
}