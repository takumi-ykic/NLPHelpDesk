using Microsoft.AspNetCore.Mvc;
using NLPHelpDesk.Data;
using NLPHelpDesk.Data.Enums;
using NLPHelpDesk.Data.Models;

namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that manages tickets.
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// Retrieves a list of tickets for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="isCompleted">A flag indicating whether to retrieve completed tickets. Defaults to false.</param>
    /// <returns>A list of Ticket objects.</returns>
    Task<List<Ticket>> GetTickets(string userId, bool isCompleted = false);
    
    /// <summary>
    /// Retrieves details of a specific ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A Ticket object representing the ticket details, or null if not found.</returns>
    Task<Ticket> GetTicketDetails(string ticketId);
    
    /// <summary>
    /// Retrieves a ticket for editing.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A Ticket object suitable for editing, or null if not found.</returns>
    Task<Ticket> GetTicketEdit(string ticketId);
    
    /// <summary>
    /// Creates a new ticket.
    /// </summary>
    /// <param name="ticket">The Ticket object to create.</param>
    /// <returns>A boolean value indicating whether the ticket creation was successful.</returns>
    Task<bool> CreateTicket(Ticket ticket);
    
    /// <summary>
    /// Updates an existing ticket.
    /// </summary>
    /// <param name="newTicket">The Ticket object containing the updated information.</param>
    /// <returns>A boolean value indicating whether the ticket update was successful.</returns>
    Task<bool> UpdateTicket(Ticket newTicket);

    /// <summary>
    /// Updates an existing ticket's category and priority.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="ticketId">The ID of the ticket to update.</param>
    /// <param name="categoryId">The new category ID.</param>
    /// <param name="priority">The new priority.</param>
    /// <returns>A boolean value indicating whether the ticket update was successful.</returns>
    Task<bool> UpdateTicket(string ticketId, int categoryId, Priority priority);
        
    /// <summary>
    /// Creates a user-ticket association.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A boolean value indicating whether the association creation was successful.</returns>;
    Task<bool> CreateUserTicket(string ticketId);
    
    /// <summary>
    /// Creates a user-ticket association for a specific user.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <param name="userId">The ID of the user to associate with the ticket.</param>
    /// <returns>A boolean value indicating whether the association creation was successful.</returns>
    Task<bool> CreateUserTicket(string ticketId, string userId);
    
    /// <summary>
    /// Creates a ticket completion record.
    /// </summary>
    /// <param name="ticketCompletion">The TicketCompletion object.</param>
    /// <param name="ticket">The Ticket object related to the completion.</param>
    /// <returns>A boolean value indicating whether the ticket completion creation was successful.</returns>
    Task<bool> CreateTicketCompletion(TicketCompletion ticketCompletion, Ticket ticket);
    
    /// <summary>
    /// Deletes a ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the CreateUserTicketicket to delete.</param>
    /// <returns>A boolean value indicating whether the ticket deletion was successful.</returns>
    Task<bool> DeleteTicket(string ticketId);
    
    /// <summary>
    /// Checks if a ticket with the given ID exists.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to check.</param>
    /// <returns>True if a ticket with the given ID exists, false otherwise.</returns>
    Task<bool> IsExistingTicket(string ticketId);
    
    /// <summary>
    /// Retrieves details of a ticket's completion information.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A Ticket object containing completion details, or null if not found.</returns>
    Task<Ticket> GetTicketCompletionDetails(string ticketId);
}