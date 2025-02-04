// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// A page model for managing the user's profile information.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IHelpDeskCategoryService _helpDeskCategoryService;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModel"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign-in manager.</param>
        /// <param name="helpDeskCategoryService">The help desk category service.</param>
        public IndexModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IHelpDeskCategoryService helpDeskCategoryService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _helpDeskCategoryService = helpDeskCategoryService;
        }
        
        /// <summary>
        /// Gets or sets the status message to display to the user.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the input model for updating the profile.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }
        
        /// <summary>
        /// Gets or sets the list of help desk categories for the dropdown.
        /// </summary>
        public List<SelectListItem> Categories { get; set; }
        
        /// <summary>
        /// Represents the input model for updating a user's profile.
        /// </summary>
        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
            public string? SelectedCategory { get; set; }
        }

        /// <summary>
        /// Handles the GET request for the profile management page.
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

            // Load categories for the dropdown.
            Categories = await _helpDeskCategoryService.GetHelpDeskCategories();

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                SelectedCategory = user.HelpDeskCategoryId.ToString()
            };
            
            return Page();
        }

        /// <summary>
        /// Handles the POST request for updating the user's profile.
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

            // Check if the submitted form data is valid.
            if (!ModelState.IsValid)
            {
                // Repopulate the form with current values if validation fails.
                Input = new InputModel()
                {
                    FirstName = user.FirstName.ToString(),
                    LastName = user.LastName.ToString(),
                    SelectedCategory = user.HelpDeskCategoryId.ToString()
                };
                
                // Generic error message.
                ModelState.AddModelError(string.Empty, "Invalid update your profile.");
                // Reload categories.
                Categories = await _helpDeskCategoryService.GetHelpDeskCategories();
                
                // Return to the page with errors.
                return Page();
            }

            // Update user properties only if they have changed.
            if (Input.FirstName != null)
            {
                user.FirstName = Input.FirstName.Trim(); 
            }

            if (Input.LastName != null)
            {
                user.LastName = Input.LastName.Trim();
            }

            if (Input.SelectedCategory != null)
            {
                user.HelpDeskCategoryId = int.Parse(Input.SelectedCategory);
            }
            
            // Update the user in the database.
            await _userManager.UpdateAsync(user);
            // Refresh the user's sign-in status.
            await _signInManager.RefreshSignInAsync(user);
            
            // Set a success message.
            StatusMessage = "Your profile has been updated";

            // Redirect to the same page to display the success message.
            return RedirectToPage();
        }
    }
}
