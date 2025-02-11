using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Data;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Data.Models;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="IHelpDeskCategoryService"/> interface to manage help desk categories.
/// </summary>
public class HelpDeskCategoryService: IHelpDeskCategoryService
{
    private readonly ApplicationContext _context;
    private readonly ILogger<HelpDeskCategoryService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpDeskCategoryService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public HelpDeskCategoryService(ApplicationContext context, ILogger<HelpDeskCategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Retrieves a list of help desk categories as SelectListItems for use in dropdowns or other UI elements.
    /// </summary>
    /// <returns>A list of <see cref="SelectListItem"/> objects representing the help desk categories.</returns>
    public async Task<List<SelectListItem>> GetHelpDeskCategories()
    {
        try
        {
            // Return help desk categories
            return await _context.HelpDeskCategories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .AsNoTracking()
                .ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving help desk categories.");
            return new List<SelectListItem>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving help desk categories.");
            return new List<SelectListItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving help desk categories.");
            return new List<SelectListItem>();
        }
    }

    /// <summary>
    /// Retrieves a help desk category by its name.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="categoryName">The name of the help desk category to retrieve.</param>
    /// <returns>A <see cref="HelpDeskCategory"/> object, or null if not found.</returns>
    public async Task<HelpDeskCategory> GetHelpDeskCategory(string categoryName)
    {
        // Log a warning if the categoryName is invalid.
        if (string.IsNullOrEmpty(categoryName))
        {
            _logger.LogWarning("Invalid or empty categoryName provided.");
            return null;
        }

        try
        {
            // Return specific help desk category
            return await _context.HelpDeskCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryName == categoryName);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving help desk category.");
            return null;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving help desk category.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving help desk category.");
            return null;
        }
    }
}