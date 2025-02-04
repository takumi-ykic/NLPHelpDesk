using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NLPHelpDesk.Models;

/// <summary>
/// Represents a comment on a ticket.
/// </summary>
public class Comment
{
    public int CommentId { get; set; }
    public string? TicketId { get; set; }
    public string? UserId { get; set; }
    
    [StringLength(500)]
    [DisplayName("Comment")]
    public string? CommentText { get; set; }
    public DateTime? CreateDate { get; set; }
    
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? FileUrl { get; set; }

    public int Delete { get; set; } = 0;
    
    public Ticket? Ticket { get; set; }
    public AppUser? AppUser { get; set; }
}