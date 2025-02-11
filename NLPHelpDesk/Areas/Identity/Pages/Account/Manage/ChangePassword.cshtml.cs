// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLPHelpDesk.Data.Models;

namespace NLPHelpDesk.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// A page model for changing a user's password.
    /// </summary>
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordModel"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign-in manager.</param>
        /// <param name="logger">The logger.</param>
        public ChangePasswordModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets or sets the input model for changing the password.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }
        
        /// <summary>
        /// Gets or sets the status message to display to the user.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        
        /// <summary>
        /// Represents the input model for changing a password.
        /// </summary>
        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }
            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        /// <summary>
        /// Handles the GET request for the change password page.  This method checks if the user is logged in and has a password set.
        /// If the user is not found, it returns a 404 Not Found error. If the user doesn't have a password set, it redirects to the SetPassword page.
        /// </summary>
        /// <returns>An IActionResult representing the result of the operation.</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Get the currently logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // User not found, return 404.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            // Check if the user has a password set.
            if (!hasPassword)
            {
                // Redirect to SetPassword page if no password is set.
                return RedirectToPage("./SetPassword");
            }

            // Return the ChangePassword page.
            return Page();
        }

        /// <summary>
        /// Handles the POST request for changing the password. This method validates the submitted form, changes the user's password, and updates the user's sign-in status.
        /// </summary>
        /// <returns>An IActionResult representing the result of the POST request. Returns the ChangePassword page with errors if validation fails, or redirects to the same page with a success message if the password is changed successfully.</returns> 
        public async Task<IActionResult> OnPostAsync()
        {
            // Check if the submitted form data is valid.
            if (!ModelState.IsValid)
            {
                // Return the ChangePassword page with model errors if validation fails.
                return Page();
            }

            // Get the currently logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // User not found, return 404.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Attempt to change the password.
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            // Check if the password change was successful.
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    // Add errors to the model state.
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Return the ChangePassword page with errors.
                return Page();
            }

            // Refresh the user's sign-in status.
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            // Redirect to the same page (ChangePassword) to display the success message.
            return RedirectToPage();
        }
    }
}
