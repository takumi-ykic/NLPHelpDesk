using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Contexts;
using NLPHelpDesk.Enums;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="ITicketService"/> interface to manage help desk tickets.
/// </summary>
public class TicketService: ITicketService
{
    private readonly ApplicationContext _context;
    private readonly IProductService _productService;
    private readonly ILogger<TicketService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TicketService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="productService">The product service.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public TicketService(ApplicationContext context,
        IProductService productService,
        ILogger<TicketService> logger)
    {
        _context = context;
        _productService = productService;
        _logger = logger;
    }
    
    /// <summary>
    /// Retrieves a list of tickets for a specific user, optionally filtering by completion status.
    /// </summary>
    /// <param name="userId">The ID of the user.  If null or empty, retrieves all unassigned tickets.</param>
    /// <param name="isCompleted">A flag indicating whether to retrieve completed tickets. Defaults to false.</param>
    /// <returns>A list of <see cref="Ticket"/> objects.</returns>
    public async Task<List<Ticket>> GetTickets(string userId, bool isCompleted = false)
    {
        // Log a warning if the userId is invalid.
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Invalid or empty userId provided.");
            return null;
        }
        
        try
        {
            // Build the query for retrieving tickets.
            var ticketsQuery = _context.Tickets
                .Include(t => t.UserTickets)
                .Include(t => t.HelpDeskCategory)
                .Include(t => t.Product)
                .Where(t => t.Delete == 0);

            // Filter by user if userId is provided.
            if (!string.IsNullOrEmpty(userId))
            {
                // Include assigned tickets to the user, created tickets by the user or unassigned tickets
                ticketsQuery =
                    ticketsQuery.Where(t => t.UserTickets.Any(ut => ut.UserId == userId) || t.UserId == userId || t.Assigned == 0);
            }
            else
            {
                //If user id is null get unassigned tickets
                ticketsQuery = ticketsQuery.Where(t => t.Assigned == 0);
            }

            // Filter by completion status.
            ticketsQuery = isCompleted
                ? ticketsQuery.Where(t => t.Status == TicketStatus.Complete || t.Status == TicketStatus.Canceled)
                : ticketsQuery.Where(t => t.Status == TicketStatus.Active || t.Status == TicketStatus.Paused);

            // Order by issue date descending.
            ticketsQuery = ticketsQuery.OrderByDescending(t => t.IssueDate);

            // Return ticket list
            return await ticketsQuery.AsNoTracking().ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving tickets.");
            return new List<Ticket>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving tickets.");
            return new List<Ticket>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving tickets.");
            return new List<Ticket>();
        }
    }

    /// <summary>
    /// Retrieves details of a specific ticket, including related entities.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A <see cref="Ticket"/> object representing the ticket details, or null if not found.</returns>
    public async Task<Ticket> GetTicketDetails(string ticketId)
    {
        // Log a warning if the ticketId is invalid.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Invalid or empty ticketId provided.");
            return null;
        }
        
        try
        {
            // Query the database for the ticket and include related data.
            return await _context.Tickets
                .Include(t => t.CreateUser)
                .Include(t => t.HelpDeskCategory)
                .Include(t => t.Product)
                .Include(t => t.UserTickets)
                .ThenInclude(ut => ut.AppUser)
                .ThenInclude(u => u.HelpDeskCategory)
                .Include(t => t.Comments)
                .ThenInclude(c => c.AppUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving ticket details.");
            return null;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving ticket details.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving ticket details.");
            return null;
        }
    }

    /// <summary>
    /// Retrieves a ticket for editing.  This method might retrieve a simpler version of the ticket than GetTicketDetails.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A <see cref="Ticket"/> object suitable for editing, or null if not found.</returns>
    public async Task<Ticket> GetTicketEdit(string ticketId)
    {
        // Log a warning if the ticketId is invalid.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Invalid or empty ticketId provided.");
            return null;
        }
        
        try
        {
            // Query the database for the ticket.
            return await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving ticket details.");
            return null;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving ticket details.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving ticket details.");
            return null;
        }
    }
    
    /// <summary>
    /// Creates a new ticket.
    /// </summary>
    /// <param name="ticket">The <see cref="Ticket"/> object to create.</param>
    /// <returns>A boolean value indicating whether the ticket creation was successful.</returns>
    public async Task<bool> CreateTicket(Ticket ticket)
    {
        // Log a warning if the ticket object is invalid.
        if (ticket == null)
        {
            _logger.LogWarning("Invalid or empty ticket object provided.");
            return false;
        }
        
        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Generate the TicketId.
                var productCode = await _productService.GetProductCode(ticket.ProductId);
                
                ticket.TicketId = productCode.Code + "-" + productCode.Count.ToString();
                
                // Add the ticket to the database.
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();
                
                // Increment the product code count.
                var incrementResult = await _productService.UpdateProductCodeCount(ticket.ProductId);

                // Check if increasing was successful
                if (!incrementResult)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Failed to increase product code count.");
                    return false;
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while creating a ticket.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while creating a ticket.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while creating a ticket.");
                return false;
            }
        }
    }

    /// <summary>
    /// Updates an existing ticket.
    /// </summary>
    /// <param name="ticket">The <see cref="Ticket"/> object containing the updated information.</param>
    /// <returns>A boolean value indicating whether the ticket update was successful.</returns>
    public async Task<bool> UpdateTicket(Ticket ticket)
    {
        // Log a warning if the ticket object is invalid.
        if (ticket == null)
        {
            _logger.LogWarning("Invalid or empty ticket object provided.");
            return false;
        }
        
        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Update the ticket in the database.
                _context.Tickets.Update(ticket);

                // Save changes and check if any changes were actually made.
                if (await _context.SaveChangesAsync() > 0)
                {
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("No changes detected during ticket update. Transaction rolled back.");
                    return false;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "A concurrency error occurred while updating the ticket. Another user may have modified the ticket.");
                return false;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while updating the ticket.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while updating the ticket.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while updating the ticket.");
                return false;
            }
        }
    }

    /// <summary>
    /// Updates the category and priority of an existing ticket.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="ticketId">The ID of the ticket to update.</param>
    /// <param name="categoryId">The new category ID.</param>
    /// <param name="priority">The new priority.</param>
    /// <returns>A boolean value indicating whether the ticket update was successful.</returns>
    public async Task<bool> UpdateTicket(ApplicationContext context, string ticketId, int categoryId, Priority priority)
    {
        // Log a warning if the ticketId is invalid.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Invalid or empty ticketId provided.");
            return false;
        }
        
        // Use a transaction for data consistency.
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                // Retrieve the ticket for editing.
                var ticket = await GetTicketEdit(ticketId);

                if (ticket == null)
                {
                    _logger.LogWarning("Ticket not found or marked as deleted.");
                    return false;
                }
                
                // Update the ticket's category, priority, and update date.
                ticket.HelpDeskCategoryId = categoryId;
                ticket.Priority = priority;
                ticket.UpdateDate = DateTime.UtcNow;

                // Update the ticket in the database.
                context.Tickets.Update(ticket);

                // Save changes and check if any changes were actually made.
                if (await context.SaveChangesAsync() > 0)
                {
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("No changes detected during ticket update. Transaction rolled back.");
                    return false;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "A concurrency error occurred while updating the help desk category in ticket. Another user may have modified the ticket.");
                return false;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while updating the help desk category in ticket.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while updating the help desk category in ticket.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while updating the help desk category in ticket.");
                return false;
            }
        }
    }

    /// <summary>
    /// Creates a user-ticket association, automatically assigning a user from the ticket's category.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A boolean value indicating whether the user-ticket association creation was successful.</returns>
    public async Task<bool> CreateUserTicket(string ticketId)
    {
        // Log a warning if the ticketId is invalid.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Invalid or empty ticketId provided.");
            return false; 
        }

        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Retrieve the ticket and its category.
                var ticket = await _context.Tickets
                    .Include(t => t.HelpDeskCategory)
                    .Where(t => t.TicketId == ticketId)
                    .Where(t => t.Delete == 0)
                    .FirstOrDefaultAsync();

                if (ticket == null || ticket.HelpDeskCategoryId == null)
                {
                    _logger.LogWarning("Ticket not found, marked as deleted, or has no category assigned.");
                    return false;
                }

                // Find a user in the ticket's category with the fewest assigned tickets.
                var userId = await _context.Users
                    .Where(u => u.HelpDeskCategoryId == ticket.HelpDeskCategoryId)
                    .GroupJoin(
                        _context.UserTickets,
                        user => user.Id,
                        ticket => ticket.UserId,
                        (user, tickets) => new
                        {
                            UserId = user.Id,
                            Count = tickets.Count()
                        })
                    .OrderBy(result => result.Count)
                    .Select(result => result.UserId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (userId == null)
                {
                    _logger.LogWarning("No users found in the assigned help desk category.");
                    return false;
                }

                // Create the user-ticket association.
                var userTicket = new UserTicket()
                {
                    UserId = userId.ToString(),
                    TicketId = ticketId
                };

                // Mark the ticket as assigned.
                ticket.Assigned = 1;

                // Update the ticket and add the user-ticket association to the database.
                _context.Tickets.Update(ticket);
                _context.UserTickets.Add(userTicket);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while creating a user ticket.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while creating a user ticket.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while creating a user ticket.");
                return false;
            }
        }
    }

    /// <summary>
    /// Creates a user-ticket association for a specific user.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <param name="userId">The ID of the user to associate with the ticket.</param>
    /// <returns>A boolean value indicating whether the user-ticket association creation was successful.</returns>
    public async Task<bool> CreateUserTicket(string ticketId, string userId)
    {
        // Log a warning if the ticketId or userId is invalid.
        if (string.IsNullOrEmpty(ticketId) || string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Invalid or empty ticketId provided.");
            return false;
        }

        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Retrieve the ticket.
                var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId && t.Delete == 0);
                
                // Create the user-ticket association.
                var userTicket = new UserTicket
                {
                    UserId = userId,
                    TicketId = ticketId
                };
                
                // Mark the ticket as assigned.
                ticket.Assigned = 1;

                // Update the ticket and add the user-ticket association to the database.
                _context.Tickets.Update(ticket);
                _context.UserTickets.Add(userTicket);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while creating a user ticket.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while creating a user ticket.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while creating a user ticket.");
                return false;
            }
        }
    }

    /// <summary>
    /// Creates a ticket completion record and updates the associated ticket.
    /// </summary>
    /// <param name="ticketCompletion">The <see cref="TicketCompletion"/> object.</param>
    /// <param name="ticket">The associated <see cref="Ticket"/> object.</param>
    /// <returns>A boolean value indicating whether the operation was successful.</returns>
    public async Task<bool> CreateTicketCompletion(TicketCompletion ticketCompletion, Ticket ticket)
    {
        // Log a warning if the ticketCompletion or ticket object is invalid.
        if (ticketCompletion == null || ticket == null)
        {
            _logger.LogWarning("Invalid or empty ticketcompletion object provided.");
            return false;
        }

        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Update the ticket and add the ticket completion record to the database.
                _context.Tickets.Update(ticket);
                _context.TicketCompletions.Add(ticketCompletion);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A concurrency error occurred while updating ticket and ticket completion. Another user may have modified them.");
                return false;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while creating a ticket completion.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while creating a ticket completion.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while creating a ticket completion.");
                return false;
            }
        }
    }

    /// <summary>
    /// Marks a ticket as deleted (soft delete).
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to delete.</param>
    /// <returns>A boolean value indicating whether the ticket deletion was successful.</returns>
    public async Task<bool> DeleteTicket(string ticketId)
    {
        // Log a warning if the ticketId is invalid.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Invalid or empty ticketId provided.");
            return false;
        }

        // Retrieve the ticket for editing 
        var ticket = await GetTicketEdit(ticketId);
        if (ticket == null)
        {
            _logger.LogError("Not found ticket with {TicketID}", ticketId);
            return false;
        }

        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Mark the ticket as deleted.
                ticket.Delete = 1;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "A concurrency error occurred while deleting the ticket. Another user may have modified the ticket.");
                return false;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while deleting the ticket.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while deleting the ticket.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while deleting the ticket.");
                return false;
            }
        }
    }
    
    /// <summary>
    /// Checks if a ticket with the given ID exists and is not marked as deleted.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to check.</param>
    /// <returns>True if the ticket exists and is not deleted, false otherwise.</returns>
    public async Task<bool> IsExistingTicket(string ticketId)
    {
        try
        {
            return await _context.Tickets.AnyAsync(t => t.TicketId == ticketId && t.Delete == 0);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while checking for existing ticket.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while checking for existing ticket.");
            return false;
        }
    }

    /// <summary>
    /// Retrieves details of a specific ticket, including related entities and the ticket completion details.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A <see cref="Ticket"/> object representing the ticket details and completion information, or null if not found.</returns>
    public async Task<Ticket> GetTicketCompletionDetails(string ticketId)
    {
        // Log a warning if the ticketId is invalid.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Invalid or empty ticketId provided.");
            return null;
        }
        
        try
        {
            // Query the database for the ticket and include related data.
            return await _context.Tickets
                .Include(t => t.TicketCompletion)
                .Include(t => t.CreateUser)
                .Include(t => t.HelpDeskCategory)
                .Include(t => t.Product)
                .Include(t => t.UserTickets)
                .ThenInclude(ut => ut.AppUser)
                .ThenInclude(u => u.HelpDeskCategory)
                .Include(t => t.Comments)
                .ThenInclude(c => c.AppUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving ticket completion.");
            return null;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving ticket completion.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving ticket completion.");
            return null;
        }
    }
}