using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents a user in the application, extending the IdentityUser class.
/// </summary>
public class AppUser: IdentityUser
{
    [StringLength(20)]
    [DisplayName("First Name")]
    public string? FirstName { get; set; }
    [StringLength(20)]
    [DisplayName("Last Name")]
    public string? LastName { get; set; }
    public int? HelpDeskCategoryId { get; set; }
    
    public int Delete { get; set; } = 0;
    
    public HelpDeskCategory? HelpDeskCategory { get; set; }
    public ICollection<Ticket>? CreateTickets { get; set; }
    public ICollection<UserTicket>? UserTickets { get; set; }
    public ICollection<TicketCompletion>? TicketCompletions { get; set; }
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<Ticket>? UpdateTickets { get; set; }
    public ICollection<Product>? CreateProducts { get; set; }
    public ICollection<Product>? UpdateProducts { get; set; }
}