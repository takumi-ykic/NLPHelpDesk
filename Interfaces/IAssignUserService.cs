using Microsoft.AspNetCore.Mvc;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that manages the assignment of users to tickets.
/// </summary>
public interface IAssignUserService
{
    /// <summary>
    /// Retrieves a list of users who can be assigned to a specific ticket within a given category.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <param name="categoryId">The ID of the category.</param>
    /// <returns>A list of <see cref="AppUser"/> objects that can be assigned to the ticket.</returns>
    Task<List<AppUser>> GetAssignableUsers(string ticketId, int categoryId);
    
    /// <summary>
    /// Assigns a user to a ticket.
    /// </summary>
    /// <param name="userTicket">An object representing the assignment of a user to a ticket.</param>
    /// <returns>A boolean value indicating whether the assignment was successful.</returns>
    Task<bool> AssignUser(UserTicket userTicket);
    
    /// <summary>
    /// Removes the assignment of a user from a ticket.
    /// </summary>
    /// <param name="userId">The ID of the user to unassign.</param>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A boolean value indicating whether the unassignment was successful.</returns>
    Task<bool> DeleteAssignedUser(string userId, string ticketId);
}