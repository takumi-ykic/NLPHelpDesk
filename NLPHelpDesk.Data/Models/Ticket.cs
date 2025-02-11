using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NLPHelpDesk.Data.Enums;

namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents a ticket.
/// </summary>
public class Ticket
{
    [StringLength(20)]
    [DisplayName("Ticket-ID")]
    public string? TicketId { get; set; }
    public string? UserId { get; set; }
    [StringLength(70, MinimumLength = 6)]
    [DisplayName("Ticket Title")]
    public string? TicketTitle { get; set; }
    [StringLength(400)]
    [DisplayName("Ticket Description")]
    public string? TicketDescription { get; set; }
    [DisplayName("Ticket Status")]
    public TicketStatus Status { get; set; } = TicketStatus.Active;
    public Priority Priority { get; set; } = Priority.Medium;
    
    [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
    [DisplayName("Issue Date")]
    public DateTime? IssueDate { get; set; }
    [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
    [DisplayName("Update Time")]
    public DateTime? UpdateDate { get; set; }
    public string? UpdateUserId { get; set; }
    [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
    [DisplayName("Complete Date")]
    public DateTime? CompleteDate { get; set; }
    
    public int? HelpDeskCategoryId { get; set; }
    public string? ProductId { get; set; }

    public int Assigned { get; set; } = 0;
    
    public int Delete { get; set; } = 0;
    
    public TicketCompletion? TicketCompletion { get; set; }
    public HelpDeskCategory? HelpDeskCategory { get; set; }
    public Product? Product { get; set; }
    public AppUser? CreateUser { get; set; }
    public AppUser? UpdateUser { get; set; }
    public ICollection<UserTicket>? UserTickets { get; set; }
    public ICollection<Comment>? Comments { get; set; }
}