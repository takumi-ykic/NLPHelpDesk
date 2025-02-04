using Microsoft.AspNetCore.Identity;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Helpers;

/// <summary>
/// Provides helper methods for working with user data.
/// </summary>
public class UserHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;

    /// <summary>
    /// Initializes a new instance of the UserHelper class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="userManager">The user manager.</param>
    public UserHelper(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    /// <summary>
    /// Gets the currently logged-in user.
    /// </summary>
    /// <returns>The currently logged-in AppUser object, or null if no user is logged in.</returns>
    public async Task<AppUser?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Check for null HttpContext and User.
        if (httpContext == null || httpContext.User == null)
        {
            // If they are null, return null
            return null;
        }
        else
        {
            // Return user information
            return await _userManager.GetUserAsync(httpContext.User);
        }
    }

    /// <summary>
    /// Gets the role of a given user.
    /// </summary>
    /// <param name="user">The AppUser object.</param>
    /// <returns>The user's role as a string, or an empty string if no role is found or the user is null.</returns>
    public async Task<String> GetUserRoleAsync(AppUser user)
    {
        if (user == null)
        {
            // Return null
            return null;
        }
        
        // Retrieve roll from user manager
        var roles = await _userManager.GetRolesAsync(user);

        // Return user roll.
        return roles.FirstOrDefault() ?? string.Empty;
    }
}