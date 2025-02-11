using Microsoft.AspNetCore.Mvc.Rendering;

namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that manages user roles.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Retrieves a list of user roles formatted as SelectListItem objects.
    /// This is typically used for populating dropdown lists in user interfaces.
    /// </summary>
    /// <returns>A list of SelectListItem objects representing the user roles.</returns>
    Task<List<SelectListItem>> GetRoles();
}