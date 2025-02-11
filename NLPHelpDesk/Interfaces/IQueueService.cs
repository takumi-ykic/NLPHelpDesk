namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that manages messages in a queue (e.g., Azure Queue Storage).
/// </summary>
public interface IQueueService
{
    /// <summary>
    /// Enqueues a ticket message asynchronously.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to enqueue.</param>
    /// <param name="userId">The ID of the user associated with the ticket (optional).</param>
    /// <param name="role">The role of the user submitting the ticket.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnqueueTicketAsync(string ticketId, string userId, string role);
}