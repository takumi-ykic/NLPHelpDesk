using Microsoft.AspNetCore.Mvc.Rendering;
using NLPHelpDesk.Data;
using NLPHelpDesk.Data.Models;

namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that manages help desk categories.
/// </summary>
public interface IHelpDeskCategoryService
{
    /// <summary>
    /// Retrieves a list of help desk categories formatted as SelectListItem objects.
    /// This is typically used for populating dropdown lists in user interfaces.
    /// </summary>
    /// <returns>A list of SelectListItem objects representing the help desk categories.</returns>
    Task<List<SelectListItem>> GetHelpDeskCategories();
    
    /// <summary>
    /// Retrieves a help desk category by its name.
    /// </summary>
    /// <param name="categoryName">The name of the help desk category to retrieve.</param>
    /// <returns>A HelpDeskCategory object if found, otherwise null.</returns>
    Task<HelpDeskCategory> GetHelpDeskCategory(string categoryName);
}