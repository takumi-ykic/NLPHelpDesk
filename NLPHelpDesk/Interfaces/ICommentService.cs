using NLPHelpDesk.Data.Models;

namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that manages comments on tickets.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Retrieves a list of comments for a specific ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A list of Comment objects associated with the ticket.</returns>
    Task<List<Comment>> GetComments(string ticketId);
    
    /// <summary>
    /// Creates a new comment on a ticket.
    /// </summary>
    /// <param name="userId">The ID of the user posting the comment.</param>
    /// <param name="ticketId">The ID of the ticket to add the comment to.</param>
    /// <param name="commentText">The text content of the comment.</param>
    /// <param name="file">An optional IFormFile representing an attached file.</param>
    /// <returns>The newly created Comment object.</returns>
    Task<Comment> CreateComment(string userId, string ticketId, string commentText, IFormFile? file);
}