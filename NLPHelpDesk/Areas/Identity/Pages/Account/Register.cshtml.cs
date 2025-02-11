// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Data.Models;
using static NLPHelpDesk.Helpers.Constants;

namespace NLPHelpDesk.Areas.Identity.Pages.Account
{
    /// <summary>
    /// This PageModel handles user registration functionality.
    /// </summary>
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IRoleService _roleService;
        private readonly IHelpDeskCategoryService _helpDeskCategoryService;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;

        /// <summary>
        /// Constructor for the RegisterModel.
        /// </summary>
        /// <param name="userManager">The UserManager instance.</param>
        /// <param name="userStore">The IUserStore instance.</param>
        /// <param name="roleService">The IRoleService instance.</param>
        /// <param name="helpDeskCategoryService">The IHelpDeskCategoryService instance.</param>
        /// <param name="signInManager">The SignInManager instance.</param>
        /// <param name="logger">The ILogger instance.</param>
        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            IRoleService roleService,
            IHelpDeskCategoryService helpDeskCategoryService,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _roleService = roleService;
            _helpDeskCategoryService = helpDeskCategoryService;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
        }
        
        /// <summary>
        /// Input model for the registration form. Uses Data Annotations for validation.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }
        
        /// <summary>
        /// URL to redirect to after successful registration.
        /// </summary>
        public string ReturnUrl { get; set; }
        
        /// <summary>
        /// List of available roles for the user to select.
        /// </summary>
        public List<SelectListItem> Roles { get; set; }
        
        /// <summary>
        /// List of available help desk categories for technicians.
        /// </summary>
        public List<SelectListItem> Categories { get; set; }
        
        /// <summary>
        /// Inner class defining the structure of the registration input.
        /// </summary>
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            [Required]
            [StringLength(40)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            [Required]
            [StringLength(40)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
            public string? SelectedRole { get; set; }
            public string? SelectedCategory { get; set; }
        }

        /// <summary>
        /// Handles GET requests to the registration page.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            
            // Populate the roles dropdown.
            Roles = await _roleService.GetRoles();

            // Populate the categories dropdown.
            Categories = await _helpDeskCategoryService.GetHelpDeskCategories();
        }

        /// <summary>
        /// Handles POST requests (form submission) for registration.
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

                // Check for duplicate email.
                if (Input.Email != null)
                {
                    var duplicatedUser = await _userManager.FindByEmailAsync(Input.Email.Trim());
                    if (duplicatedUser != null)
                    {
                        ModelState.AddModelError(string.Empty, Input.Email.Trim() + " is already taken.");
                        return Page();
                    }

                    email = Input.Email.Trim();
                }

                // Ensure password and confirm password match.
                if (Input.Password != null && Input.ConfirmPassword != null)
                {
                    if (Input.Password.Trim() != Input.ConfirmPassword.Trim())
                    {
                        ModelState.AddModelError(string.Empty, "Please match password and confirmed password both.");
                        return Page();
                    }

                    password = Input.Password.Trim();
                }

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, email, CancellationToken.None);

                user.EmailConfirmed = true;

                // Set user's first and last name
                if (Input.FirstName != null)
                {
                    user.FirstName = Input.FirstName.Trim();
                }

                if (Input.LastName != null)
                {
                    user.LastName = Input.LastName.Trim();
                }

                // Handle technician category selection.
                if (Input.SelectedRole != null && Input.SelectedRole == ROLE_TECHNICIAN)
                {
                    if (Input.SelectedCategory != null)
                    {
                        user.HelpDeskCategoryId = int.Parse(Input.SelectedCategory);
                    }
                }

                // Create the user with the provided password.
                if (password != null)
                {
                    var result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");
                        
                        // Add the user to the selected role.
                        if (Input.SelectedRole != null)
                        {
                            await _userManager.AddToRoleAsync(user, Input.SelectedRole);
                        }
                        
                        // Sign in the newly registered user.
                        await _signInManager.SignInAsync((AppUser)user, isPersistent: false);

                        return RedirectToAction(nameof(Index), "Tickets");
                    }
                    else
                    {
                        // Add errors to the model state if user creation fails.
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid registration.");
            }
            
            // If we got this far, something failed, redisplay form
            Roles = await _roleService.GetRoles();
            Categories = await _helpDeskCategoryService.GetHelpDeskCategories();
            return Page();
        }

        /// <summary>
        /// Creates a new instance of the AppUser class.
        /// </summary>
        /// <returns>A new AppUser instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if an instance of AppUser cannot be created.</exception>
        private AppUser CreateUser()
        {
            try
            {
                // Use Activator.CreateInstance to create a new AppUser.  This relies on the AppUser
                // class having a parameterless constructor.
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                // Throw an exception if the AppUser cannot be created.  This usually happens
                // if the AppUser class is abstract or doesn't have a parameterless constructor.
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        /// <summary>
        /// Gets the IUserEmailStore for the AppUser.
        /// </summary>
        /// <returns>The IUserEmailStore instance.</returns>
        /// <exception cref="NotSupportedException">Thrown if the user store does not support email functionality.</exception>
        private IUserEmailStore<AppUser> GetEmailStore()
        {
            // Check if the user manager supports email functionality.
            if (!_userManager.SupportsUserEmail)
            {
                // If email support is not available, throw an exception.  The default UI for
                // Identity requires email support.
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            
            // Cast the user store to an IUserEmailStore.  This is safe to do because of the check above.
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
