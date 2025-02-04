// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Areas.Identity.Pages.Account
{
    /// <summary>
    /// This PageModel handles user logout functionality.
    /// </summary>
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        /// <summary>
        /// Constructor for the LogoutModel.
        /// </summary>
        /// <param name="signInManager">The SignInManager instance.</param>
        /// <param name="logger">The ILogger instance.</param>
        public LogoutModel(SignInManager<AppUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Handles POST requests for user logout.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after logout.</param>
        /// <returns>An IActionResult representing the result of the operation.</returns>
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Sign the user out.
            await _signInManager.SignOutAsync();
            
            _logger.LogInformation("User logged out.");
            
            // Redirect the user.
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}
