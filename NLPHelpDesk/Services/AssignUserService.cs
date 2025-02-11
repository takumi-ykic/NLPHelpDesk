using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Data;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Data.Models;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="IAssignUserService"/> interface to manage the assignment of users to tickets.
/// </summary>
public class AssignUserService: IAssignUserService
{
    private readonly ApplicationContext _context;
    private readonly ILogger<AssignUserService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssignUserService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public AssignUserService(ApplicationContext context, ILogger<AssignUserService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Retrieves a list of users who can be assigned to a specific ticket within a given category.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <param name="categoryId">The ID of the category.</param>
    /// <returns>A list of <see cref="AppUser"/> objects that can be assigned to the ticket.</returns>
    public async Task<List<AppUser>> GetAssignableUsers(string ticketId, int categoryId)
    {
        // Log a warning if the ticketId is invalid.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Invalid or empty ticketId provided.");
            // Return empty list
            return new List<AppUser>();
        }
        
        try
        {
            // Query the database for users who can be assigned to the ticket.
            return await _context.Users
                .Include(u => u.HelpDeskCategory)
                .Where(u => u.HelpDeskCategoryId != null)
                .Where(u => !u.UserTickets!.Any(ut => ut.TicketId == ticketId))
                .OrderBy(u => u.HelpDeskCategoryId == categoryId ? 0 : 1)
                .ThenBy(u => u.HelpDeskCategoryId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving assignable users.");
            return new List<AppUser>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving assignable users.");
            return new List<AppUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving assignable users.");
            return new List<AppUser>();
        }
    }

    /// <summary>
    /// Assigns a user to a ticket.
    /// </summary>
    /// <param name="userTicket">An object representing the assignment of a user to a ticket.</param>
    /// <returns>A boolean value indicating whether the assignment was successful.</returns>
    public async Task<bool> AssignUser(UserTicket userTicket)
    {
        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Retrieve the ticket.
                var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == userTicket.TicketId);
                
                // Update the ticket's assigned status if it's not already assigned.
                if (ticket.Assigned == 0)
                {
                    ticket.Assigned = 1;
                }

                // Add the user-ticket association.
                _context.Tickets.Update(ticket);
                _context.UserTickets.Add(userTicket);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while creating an userticket.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while creating an userticket.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while creating an userticket.");
                return false;
            }
        }
    }

    /// <summary>
    /// Removes the assignment of a user from a ticket.
    /// </summary>
    /// <param name="userId">The ID of the user to unassign.</param>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A boolean value indicating whether the unassignment was successful.</returns>
    public async Task<bool> DeleteAssignedUser(string userId, string ticketId)
    {
        // Log a warning if the ticketId or userId is invalid.
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Invalid or empty ticketId or userId provided.");
            return false;
        }
        
        try
        {
            // Retrieve the user-ticket association.
            var userTicket = await _context.UserTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TicketId == ticketId);

            if (userTicket == null)
            {
                _logger.LogError("Not found an userticket object");
                return false;
            }

            // Remove the user-ticket association.
            _context.UserTickets.Remove(userTicket);

            // Use a transaction for data consistency.
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    
                    // Check if any UserTickets remain for the given ticketId.
                    var countUserTickets = await _context.UserTickets.CountAsync(ut => ut.TicketId == ticketId);
                    if (countUserTickets == 0)
                    {
                        var ticket = await _context.Tickets.FindAsync(ticketId);
                        if (ticket != null && ticket.Assigned == 1)
                        {
                            // If no UserTickets remain, update the ticket's assigned status.
                            ticket.Assigned = 0;
                            _context.Tickets.Update(ticket);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"Ticket {ticketId} unassigned successfully.");
                        }
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Exception occurred while deleting an assigned user.");
                    return false;
                }
                catch (SqlException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Exception occurred while deleting an assigned user.");
                    return false;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Exception occurred while deleting an assigned user.");
                    return false;
                }
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Exception occurred while deleting an assigned user.");
            return false;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Exception occurred while deleting an assigned user.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while deleting an assigned user.");
            return false;
        }
    }
}