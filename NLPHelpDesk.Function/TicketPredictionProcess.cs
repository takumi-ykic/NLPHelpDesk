using System.Text;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NLPHelpDesk.Data.Enums;
using NLPHelpDesk.Data.Models;
using NLPHelpDesk.Interfaces;
using static NLPHelpDesk.Helpers.Constants;

namespace NLPHelpDesk.Function;

/// <summary>
/// This class defines the Azure Function that processes ticket prediction requests from a queue.
/// </summary>
public class TicketPredictionProcess
{
    private readonly ITicketService _ticketService;
    private readonly IHelpDeskCategoryService _helpDeskCategoryService;
    private readonly ICategoryPredictionService _categoryPredictionService;
    private readonly IPriorityPredictionService _priorityPredictionService;
    private readonly ILogger<TicketPredictionProcess> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TicketPredictionProcess"/> class.
    /// </summary>
    /// <param name="ticketService">The ticket service.</param>
    /// <param name="helpDeskCategoryService">The help desk category service.</param>
    /// <param name="categoryPredictionService">The category prediction service.</param>
    /// <param name="priorityPredictionService">The priority prediction service.</param>
    /// <param name="logger">The logger.</param>
    public TicketPredictionProcess(ITicketService ticketService,
        IHelpDeskCategoryService helpDeskCategoryService,
        ICategoryPredictionService categoryPredictionService,
        IPriorityPredictionService priorityPredictionService,
        ILogger<TicketPredictionProcess> logger)
    {
        _ticketService = ticketService;
        _helpDeskCategoryService = helpDeskCategoryService;
        _categoryPredictionService = categoryPredictionService;
        _priorityPredictionService = priorityPredictionService;
        _logger = logger;
    }

    /// <summary>
    /// Runs the Azure Function to process a ticket prediction request.
    /// </summary>
    /// <param name="message">The queue message containing the ticket information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Function("TicketPredictionProcess")]
    public async Task RunAsync(
        [QueueTrigger("nlphelpdesk-ticket-prediction", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        try
        {
            string decodedMessage = message.MessageText;

            // Deserialize the JSON string
            var data = JsonSerializer.Deserialize<TicketQueueMessage>(decodedMessage);

            try
            {
                // Validate the message.
                if (data == null)
                {
                    _logger.LogWarning("Invalid message received");
                    return;
                }

                _logger.LogInformation($"Processing Ticket {data.TicketId}");

                // Retrieve the ticket
                var ticket = await _ticketService.GetTicketEdit(data.TicketId);
                if (ticket == null)
                {
                    _logger.LogWarning($"Ticket {data.TicketId} not found.");
                    return;
                }

                // Prepare input for prediction
                var inputData = new CsvData
                {
                    Text = $"{ticket.TicketTitle},{ticket.TicketDescription}"
                };

                // Predict category
                var categoryPrediction = await _categoryPredictionService.GetCategoryPrediction(inputData);
                var category = await _helpDeskCategoryService.GetHelpDeskCategory(categoryPrediction.PredictedCategory);

                // Predict priority
                var priorityPrediction = await _priorityPredictionService.GetPriorityPrediction(inputData);
                var priority = Enum.TryParse<Priority>(priorityPrediction.PredictedPriority, true, out var priorityEnum)
                    ? priorityEnum
                    : Priority.Low;

                // Update ticket in database
                var updateResult = await _ticketService.UpdateTicket(ticket.TicketId, category.CategoryId, priority);

                if (updateResult)
                {
                    // Assign ticket to a user
                    bool assignResult = data.Role == ROLE_TECHNICIAN
                        ? await _ticketService.CreateUserTicket(ticket.TicketId, data.UserId)
                        : await _ticketService.CreateUserTicket(ticket.TicketId); // Assign to available user

                    if (!assignResult)
                    {
                        _logger.LogError($"Failed to assign ticket {data.TicketId} to user {data.UserId ?? "Unassigned"}");
                    }
                }
                else
                {
                    _logger.LogError($"Failed to process ticket {data.TicketId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ticket.");
            }
        }
        catch (FormatException ex)
        {
            _logger.LogError($"Error decoding Base64 string: {ex.Message}");
            return;
        }
    }
}

/// <summary>
/// Represents the message received from the ticket prediction queue.
/// </summary>
public class TicketQueueMessage
{
    public string TicketId { get; set; }
    public string UserId { get; set; }
    public string Role { get; set; }
}