// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// A page model for deleting a user's personal data.
    /// </summary>
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeletePersonalDataModel"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign-in manager.</param>
        /// <param name="logger">The logger.</param>
        public DeletePersonalDataModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user is required to enter their password before deleting their data.
        /// </summary>
        public bool RequirePassword { get; set; }
        
        /// <summary>
        /// Gets or sets the input model for deleting personal data.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Represents the input model for deleting personal data.
        /// </summary>
        public class InputModel
        {

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }
        
        /// <summary>
        /// Handles the GET request for the delete personal data page.
        /// </summary>
        /// <returns>An IActionResult that represents the result of the GET request.</returns>
        public async Task<IActionResult> OnGet()
        {
            // Get the currently logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // User not found, return 404.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Check if the user has a password set.
            RequirePassword = await _userManager.HasPasswordAsync(user);
            // Return the DeletePersonalData page.
            return Page();
        }

        /// <summary>
        /// Handles the POST request for deleting personal data.
        /// </summary>
        /// <returns>An IActionResult that represents the result of the POST request.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Get the currently logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // User not found, return 404.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Check if the user has a password set.
            RequirePassword = await _userManager.HasPasswordAsync(user);
            // Only check password if one is required.
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    // Return to the page with the error.
                    return Page();
                }
            }

            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                // Throw exception if delete fails.  Consider a more graceful fallback in production.
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            // Sign the user out after deleting their data.
            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            // Redirect to the home page.
            return Redirect("~/");
        }
    }
}
