using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NLPHelpDesk.Data;
using NLPHelpDesk.Data.Models;

namespace NLPHelpDesk.Controllers;

/// <summary>
/// The home controller for the application.
/// </summary>
[AllowAnonymous] // Allow anonymous access to all actions in this controller.
public class HomeController : Controller
{
    private readonly ApplicationContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="context">The application context.</param>
    public HomeController(ApplicationContext context)
    {
        _context = context;
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
    
    /// <summary>
    /// Checks if the database is ready by attempting to establish a connection.
    /// </summary>
    /// <returns>A JSON object with a boolean property "isDatabaseReady" indicating the database status.</returns>
    [HttpGet]
    public IActionResult CheckDatabaseStatus()
    {
        // Create scope for checking database connection
        using (var scope = HttpContext.RequestServices.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            // Check if the database can connect
            bool isDatabaseReady = context.Database.CanConnect();

            bool isAuthenticated = false;
            if (isDatabaseReady)
            {
                isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            }
            
            // Return result
            return Json(new { isDatabaseReady, isAuthenticated });
        }
    }
}