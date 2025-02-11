using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using NLPHelpDesk.Data.Models;
using NLPHelpDesk.Interfaces;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="IQueueService"/> interface for interacting with Azure Queue Storage.
/// </summary>
public class QueueService : IQueueService
{
    private readonly QueueClient _queueClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueService"/> class.
    /// </summary>
    /// <param name="options">The configuration options for the queue settings.</param>
    /// <exception cref="InvalidOperationException">Thrown if the Azure Storage connection string or queue name is missing.</exception>
    public QueueService(IOptions<QueueSettings> options)
    {
        var settings = options.Value;

        // Validate configuration settings.
        if (string.IsNullOrEmpty(settings.AzureStorageConnectionString))
        {
            throw new InvalidOperationException("Azure Storage connection string is missing.");
        }

        if (string.IsNullOrEmpty(settings.PredictionQueue))
        {
            throw new InvalidOperationException("Queue name is missing.");
        }

        _queueClient = new QueueClient(settings.AzureStorageConnectionString, settings.PredictionQueue);
        _queueClient.CreateIfNotExists();
    }
    
    /// <summary>
    /// Enqueues a ticket message asynchronously.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to enqueue.</param>
    /// <param name="userId">The ID of the user associated with the ticket (optional).</param>
    /// <param name="role">The role of the user submitting the ticket.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task EnqueueTicketAsync(string ticketId, string userId, string role)
    {
        // Create the message payload.
        var message = JsonSerializer.Serialize(new { TicketId = ticketId, UserId = userId, Role = role });
        
        // Send the message to the queue with serializing to JSON and encoding Base64.
        await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message)));
    }
}