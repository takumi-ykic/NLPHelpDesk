using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Contexts;
using NLPHelpDesk.Interfaces;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="IUserService"/> interface for managing user-related operations.
/// </summary>
public class UserService: IUserService
{
    private readonly ApplicationContext _context;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public UserService(ApplicationContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Checks if a user with the given ID exists.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <returns><c>true</c> if a user with the given ID exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsExistingUser(string userId)
    {
        // Log a warning if the userId is invalid.
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Invalid or empty userId provided");
            return false;
        }
        
        try
        {
            // Query the database to check if the user exists.
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            // Return user is null nor
            return user != null;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "InvalidOperationException occurred while processing user.");
            return false;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SqlException occurred during database operation.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return false;
        }
    }
}