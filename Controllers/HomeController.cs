using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Controllers;

/// <summary>
/// The home controller for the application.
/// </summary>
[AllowAnonymous] // Allow anonymous access to all actions in this controller.
public class HomeController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="logger">The logger.</param>
    public HomeController(UserManager<AppUser> userManager, ILogger<HomeController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// The index action, which displays the home page.  Redirects to the Tickets index if the user is logged in.
    /// </summary>
    /// <returns>An IActionResult representing the result of the action.</returns>
    public IActionResult Index()
    {
        // Check if user is log in
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            // User is already logged in, redirect to the Tickets index page.
            return RedirectToAction(nameof(Index), "Tickets");
        }
        
        // User is not logged in, return the default Index view.
        return View();
    }

    /// <summary>
    /// The privacy action, which displays the privacy policy page.
    /// </summary>
    /// <returns>An IActionResult representing the result of the action.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// The error action, which displays the error page.
    /// </summary>
    /// <returns>An IActionResult representing the result of the action.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Redirects the user to the login page.
    /// </summary>
    /// <returns>An IActionResult representing the result of the redirect.</returns>
    public IActionResult ToLogin()
    {
        // Check if user is log in
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            // User is already logged in, redirect to the Tickets index page.
            return RedirectToAction(nameof(Index), "Tickets");
        }

        // Redirect to the Identity login page.
        return Redirect("/Identity/Account/Login");
    }

    /// <summary>
    /// Redirects the user to the registration/signup page.
    /// </summary>
    /// <returns>An IActionResult representing the result of the redirect.</returns>
    public IActionResult ToSignup()
    {
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            // User is already logged in, redirect to the Tickets index page.
            return RedirectToAction(nameof(Index), "Tickets");
        }

        // Redirect to the Identity registration page.
        return Redirect("/Identity/Account/Register");
    }
}