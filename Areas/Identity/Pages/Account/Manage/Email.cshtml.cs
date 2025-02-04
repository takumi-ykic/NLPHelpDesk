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
    /// A page model for managing a user's email address.
    /// </summary>
    public class EmailModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailModel"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign-in manager.</param>
        public EmailModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Gets or sets the user's current email address.
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user's email is confirmed.  (Not currently used in this code, but could be used in the view)
        /// </summary>
        public bool IsEmailConfirmed { get; set; }
        
        /// <summary>
        /// Gets or sets the status message to display to the user.
        /// </summary>
        [TempData] public string StatusMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the input model for changing the email address.
        /// </summary>
        [BindProperty] public InputModel Input { get; set; }

        /// <summary>
        /// Represents the input model for changing an email address.
        /// </summary>
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        /// <summary>
        /// Loads the user's email address into the model.
        /// </summary>
        /// <param name="user">The user.</param>
        private async Task LoadAsync(AppUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                // Pre-populate the input field with the current email.
                NewEmail = email,
            };
        }

        /// <summary>
        /// Handles the GET request for the email management page.
        /// </summary>
        /// <returns>An IActionResult that represents the result of the GET request.</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Get the currently logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // User not found, return 404.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Load the user's email.
            await LoadAsync(user);
            return Page();
        }

        /// <summary>
        /// Handles the POST request for changing the email address.
        /// </summary>
        /// <returns>An IActionResult that represents the result of the POST request.</returns>
        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            // Get the currently logged in user.
            var user = await _userManager.GetUserAsync(User);
            // Trim whitespace from the new email.
            string newEmail = Input.NewEmail.Trim();

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Check if the submitted form data is valid.
            if (!ModelState.IsValid)
            {
                // Reload the user's email for the page.
                await LoadAsync(user);
                // Return to the page with model errors.
                return Page();
            }

            // Check new email exists in database already.
            var IsEmailExisted = await _userManager.FindByEmailAsync(newEmail);
            if (IsEmailExisted != null)
            {
                // Restore the original email.
                Email = user.Email;
                ModelState.AddModelError(string.Empty, newEmail + " is already taken.");
                // Return to the page with the "email taken" error
                return Page();
            }

            var email = user.Email;
            // Only proceed if the email is actually changing.
            if (newEmail != email)
            {
                // Generate the email change token.
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
                // Attempt to change the email.
                var result = await _userManager.ChangeEmailAsync(user, newEmail, code);
                if (result.Succeeded)
                {
                    // Update the username to match the email.
                    user.UserName = newEmail;
                    // Persist the username change.
                    await _userManager.UpdateAsync(user);

                    // Refresh the user's sign-in status.
                    await _signInManager.RefreshSignInAsync(user);
                    // Redirect to the page with a success message.
                    return RedirectToPage();
                }
                else
                {
                    // Restore the original email.
                    Email = user.Email;
                    // Add a generic error message.
                    ModelState.AddModelError(string.Empty, "Error. Please try again.");
                    // Return to the page with the error.
                    return Page();
                }
            }
            else
            {
                // Set a "no change" message.
                StatusMessage = "Your email is unchanged.";
                // Redirect to the page with the message.
                return RedirectToPage();
            }
        }
    }
}
