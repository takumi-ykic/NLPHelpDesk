using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Interfaces;
using static NLPHelpDesk.Helpers.Constants;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="IRoleService"/> interface to manage user roles.
/// </summary>
public class RoleService: IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RoleService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleService"/> class.
    /// </summary>
    /// <param name="roleManager">The role manager.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public RoleService(RoleManager<IdentityRole> roleManager, ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }
    
    /// <summary>
    /// Retrieves a list of roles (excluding the administrator role) as SelectListItems.
    /// </summary>
    /// <returns>A list of <see cref="SelectListItem"/> objects representing the roles.</returns>
    public async Task<List<SelectListItem>> GetRoles()
    {
        try
        {
            // Query the database for roles, excluding the administrator role.
            return await _roleManager.Roles
                .Where(r => r.Name != ROLE_ADMIN)
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .AsNoTracking()
                .ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving roles.");
            return new List<SelectListItem>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving roles.");
            return new List<SelectListItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving roles.");
            return new List<SelectListItem>();
        }
    }
}