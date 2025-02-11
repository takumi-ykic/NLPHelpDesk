// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLPHelpDesk.Data.Models;

namespace NLPHelpDesk.Areas.Identity.Pages.Account
{
    /// <summary>
    /// This PageModel handles the user login functionality.
    /// </summary>
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Constructor for the LoginModel.
        /// </summary>
        /// <param name="signInManager">The SignInManager instance.</param>
        /// <param name="userManager">The UserManager instance.</param>
        /// <param name="logger">The ILogger instance.</param>
        public LoginModel(SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }
        
        /// <summary>
        /// Input model for the login form. Uses Data Annotations for validation.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }
        
        /// <summary>
        /// URL to redirect to after successful login.
        /// </summary>
        public string ReturnUrl { get; set; }
        
        /// <summary>
        /// Stores error messages to be displayed on the page.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Inner class defining the structure of the login input.
        /// </summary>
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        /// Handles GET requests to the login page.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task OnGetAsync(string returnUrl = null)
        {
            // Check for error messages passed via TempData.
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        /// <summary>
        /// Handles POST requests (form submission) for login.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>An IActionResult representing the result of the operation.</returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                string email = "";
                string password = "";

                if (Input.Email != null && Input.Password != null)
                {
                    email = Input.Email.Trim();
                    password = Input.Password.Trim();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
                
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        return RedirectToPage(nameof(Index), "Tickets");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
