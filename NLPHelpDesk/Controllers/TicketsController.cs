using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLPHelpDesk.Data.Enums;
using NLPHelpDesk.Helpers;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Data.Models;
using static NLPHelpDesk.Helpers.Constants;

namespace NLPHelpDesk.Controllers;

/// <summary>
/// Controller for managing tickets.  Requires authorization.
/// </summary>
[Authorize] // Requires user to be logged in
public class TicketsController : Controller
{
    private readonly ITicketService _service;
    private readonly IProductService _productService;
    private readonly ICommentService _commentService;
    private readonly IAssignUserService _assignUserService;
    private readonly IUserService _userService;
    private readonly IHelpDeskCategoryService _categoryService;
    private readonly ILogger<TicketsController> _logger;
    private readonly AppUser? _user;
    private readonly UserHelper _userHelper;
    private readonly IAzureBlobService _blobService;
    private readonly IQueueService _queueService;
    private readonly IHtmlHelper _htmlHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="TicketsController"/> class.
    /// </summary>
    /// <param name="service">The ticket service.</param>
    /// <param name="productService">The product service.</param>
    /// <param="commentService">The comment service.</param>
    /// <param name="assignUserService">The assignment service.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="categoryService">The category service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="userHelper">The user helper.</param>
    /// <param name="blobService">The Azure blob service.</param>
    /// <param name="backgroundTaskQueue">The background task queue.</param>
    /// <param name="htmlHelper">The HTML helper.</param>
    public TicketsController(ITicketService service,
        IProductService productService,
        ICommentService commentService,
        IAssignUserService assignUserService,
        IUserService userService,
        IHelpDeskCategoryService categoryService,
        ILogger<TicketsController> logger,
        UserHelper userHelper,
        IAzureBlobService blobService,
        IQueueService queueService,
        IHtmlHelper htmlHelper)
    {
        _service = service;
        _productService = productService;
        _commentService = commentService;
        _assignUserService = assignUserService;
        _userService = userService;
        _categoryService = categoryService;
        _logger = logger;
        _userHelper = userHelper;
        _blobService = blobService;
        _queueService = queueService;
        _htmlHelper = htmlHelper;

        _user = _userHelper.GetCurrentUserAsync().Result;
    }
    
    /// <summary>
    /// Displays a list of tickets for the current user.
    /// </summary>
    /// <param name="isCompleted">Optional flag to filter tickets by completion status.</param>
    /// <returns>An IActionResult representing the Index view with the ticket list.</returns>
    public async Task<IActionResult> Index(bool isCompleted = false)
    {
        // Call the service to get ticket list
        var ticketList = await _service.GetTickets(_user.Id.ToString(), isCompleted);

        // Set ViewBag for IsCompleted flag to switch displaying tickets in the list
        ViewBag.IsCompleted = isCompleted;
        TempData[TEMP_DATA_RETURN_URL] = Request.GetDisplayUrl();
        
        // Returns the Index view with the ticket list model
        return View(ticketList);
    }
    
    /// <summary>
    /// Displays details of a specific ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>An IActionResult representing the Details view, or a NotFound or BadRequest result.</returns>
    public async Task<IActionResult> Details(string ticketId)
    {
        // Validate the ticketId.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Details action called with null or empty ticketId.");
            // Returns a 400 Bad Request
            return BadRequest("Ticket ID is required.");
        }
        
        // Call the service to get ticket details
        var ticket = await _service.GetTicketDetails(ticketId);
    
        if (ticket == null)
        {
            _logger.LogWarning("Ticket with ID {TicketId} not found.", ticketId);
            // Returns a 404 Not Found
            return NotFound();
        }

        if (ticket.Comments != null && ticket.Comments.Any())
        {
            // Call GetCommentFileUrls method to get url
            GetCommetnFileUrls(ticket.Comments);
        }

        // Check if this ticket is owned by login user
        bool isOwner = ticket.UserId == _user.Id;
        
        // Check if login user is assinged to this ticket
        bool isAssigned = false;
        foreach (var userTicket in ticket.UserTickets)
        {
            isAssigned = userTicket.UserId == _user.Id;
            if (isAssigned) break;
        }

        // Create ViewModel for ticket detail page
        TicketDetailsViewModel viewModel = new TicketDetailsViewModel
        {
            Ticket = ticket,
            IsOwner = isOwner,
            IsAssigned = isAssigned
        };
        
        // Returns the Details view with the ViewModel
        return View(viewModel);
    }
    
    /// <summary>
    /// Displays the ticket creation form.
    /// </summary>
    /// <returns>An IActionResult representing the ticket creation view.</returns>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Call the service to retrieve product select list items
            var productSelectListItems = await _productService.GetProductSelectListItems();
            
            // Store the product select list items in ViewBag for use in the view.
            ViewBag.Products = productSelectListItems;

            // Returns the Create view
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving products for ticket creation.");
            TempData[TEMP_DATA_ERROR_MESSAGE] = "An error occurred. Please try again later.";
            // Redirect to the Index action with an error message
            return RedirectToAction(nameof(Index));
        }
    }
    
    /// <summary>
    /// Handles the creation of a new ticket.
    /// </summary>
    /// <param name="newTicket">The Ticket object containing the new ticket data.</param>
    /// <returns>An IActionResult representing the redirect to the ticket list or the create view if there are errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ticket newTicket)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Invalid input.");
            // Return the view with the entered data 
            return View(newTicket);
        }

        try
        {
            // Get the current user's ID and role.
            string userId = _user.Id;
            string role = await _userHelper.GetUserRoleAsync(_user);

            newTicket.TicketDescription = newTicket.TicketDescription.Trim();
            newTicket.UserId = userId;
            newTicket.IssueDate = DateTime.UtcNow;

            // Set default product if none selected
            if (string.IsNullOrEmpty(newTicket.ProductId))
            {
                newTicket.ProductId = PRODUCT_CODE_DEFAULT;
            }
            
            // Call the service to create a new ticket
            var result = await _service.CreateTicket(newTicket);

            // Check if the ticket creation was successful
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to create ticket.");
                // Call the service to repopulate the product select list and return the view with the entered data.
                var productSelectListItems = await _productService.GetProductSelectListItems();
                ViewBag.Products = productSelectListItems;
                return View(newTicket);
            }
            
            // Send data to queue to call prediction process in Azure Function
            await _queueService.EnqueueTicketAsync(newTicket.TicketId, userId, role);
            
            // Redirect to the Index action after successful ticket creation.
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating ticket: {Message}", ex.Message);
            // Call the service to repopulate the product select list and return the view with the entered data.
            var productSelectListItems = await _productService.GetProductSelectListItems();
            ViewBag.Products = productSelectListItems;
            return View(newTicket);
        }
    }
    
    /// <summary>
    /// Displays the ticket edit form.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to edit.</param>
    /// <returns>An IActionResult representing the ticket edit view, a BadRequest if ticketId is null or empty, or a NotFound if the ticket is not found.</returns>
    public async Task<IActionResult> Edit(string ticketId)
    {
        // Validate the ticketId.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Edit action called with null or empty ticketId.");
            // Return a 400 Bad Request
            return BadRequest("Ticket ID is required.");
        }
        
        // Call the service to retrieve the ticket for editing
        var ticket = await _service.GetTicketEdit(ticketId);

        // Check if the ticket exists
        if (ticket == null)
        {
            _logger.LogWarning("Ticket with ID {TicketId} not found.", ticketId);
            // Return a 404 Not Found
            return NotFound();
        }
        
        // Populate the ViewBag with ticket statuses for the dropdown list.
        ViewBag.TicketStatus = GetTicketStatusSelectList();
        
        // Return the Edit view with ticket information
        return View(ticket);
    }

    /// <summary>
    /// Handles the updating of an existing ticket.
    /// </summary>
    /// <param name="updateTicket">The Ticket object containing the updated ticket data.</param>
    /// <returns>An IActionResult representing the redirect to the ticket details view on success, or the edit view with errors on failure.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Ticket updateTicket)
    {
        if (!ModelState.IsValid)
        {
            // If model state is invalid, retrieve the existing ticket.
            var ticket = await _service.GetTicketEdit(updateTicket.TicketId);
            if (ticket == null)
            {
                _logger.LogWarning("Ticket with ID {TicketId} not found.", updateTicket.TicketId);
                // Return a 404 Not Found
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, "Invalid input.");
            // Call the service class to repopulate the ViewBag with ticket statuses.
            ViewBag.TicketStatus = GetTicketStatusSelectList();
            
            // Return the view with the entered data
            return View(updateTicket);
        }
        
        try
        {
            // Call the service to retrieve the existing ticket
            var ticket = await _service.GetTicketEdit(updateTicket.TicketId);

            // Check if the ticket exists
            if (ticket == null)
            {
                _logger.LogWarning("Ticket with ID {TicketId} not found.", updateTicket.TicketId);
                // Return a 404 Not Found
                return NotFound();
            }

            // Update the ticket properties that have changed.
            if (ticket.TicketDescription != updateTicket.TicketDescription)
            {
                ticket.TicketDescription = updateTicket.TicketDescription;
            }

            if (ticket.Status != updateTicket.Status)
            {
                ticket.Status = updateTicket.Status;
            }

            if (ticket.Priority != updateTicket.Priority)
            {
                ticket.Priority = updateTicket.Priority;
            }
            
            ticket.UpdateUserId = _user.Id;
            ticket.UpdateDate = DateTime.UtcNow;
            
            // Call the service to update the ticket
            var result = await _service.UpdateTicket(ticket);

            // Check if the update was successful.
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to update ticket.");
                // Repopulate the ViewBag with ticket statuses.
                ViewBag.TicketStatus = GetTicketStatusSelectList();
                
                // Return the view with the entered data.
                return View(updateTicket);
            }

            // Redirect to the ticket details page after successful update.
            return RedirectToAction(nameof(Details), new { ticketId = ticket.TicketId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while editing ticket {TicketId}", updateTicket.TicketId);
            // Repopulate the ViewBag with ticket statuses.
            ViewBag.TicketStatus = GetTicketStatusSelectList();
            
            // Return the view with the entered data.
            return View(updateTicket);
        }
    }
    
    /// <summary>
    /// Displays the confirmation view for deleting a ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to delete.</param>
    /// <returns>An IActionResult representing the delete confirmation view, a BadRequest if ticketId is null or empty, or a NotFound if the ticket is not found.</returns>
    public async Task<IActionResult> Delete(string ticketId)
    {
        // Validate the ticketId.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Delete action called with null or empty ticketId.");
            // Return 400 Bad Request
            return BadRequest("Ticket ID is required.");
        }

        // Call the service to retrieve the ticket details for display in the confirmation view
        var ticket = await _service.GetTicketDetails(ticketId);
        // Check if the ticket exists
        if (ticket == null)
        {
            _logger.LogWarning("Ticket with ID {TicketId} not found.", ticketId);
            // Return 400 Bad Request
            return NotFound();
        }

        // Return the delete confirmation view
        return View(ticket);
    }
    
    /// <summary>
    /// Handles the actual deletion of a ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to delete.</param>
    /// <returns>An IActionResult representing the redirect to the ticket list on success, or the delete confirmation view with errors on failure.</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTicket(string ticketId)
    {
        // Validate the ticketId.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Delete action called with null or empty ticketId.");
            // Return 400 Bad Request
            return BadRequest("Ticket ID is required.");
        }

        try
        {
            // Call the service to delete the ticket using the service
            var result = await _service.DeleteTicket(ticketId);
            // Check if the deletion was successful.
            if (result)
            {
                _logger.LogInformation("Success with deleting ticket with TicketID: {TicketId}", ticketId);
                // Redirect to the ticket list
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogError("Fail to delete ticket");
                ModelState.AddModelError(string.Empty, "Failed to delete ticket. Please try it again later.");
                // If deletion fails, retrieve the ticket details again to display on the view.
                var ticket = await _service.GetTicketDetails(ticketId);
                // Return the view with the model data
                return View(ticket);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting ticket {TicketId}", ticketId);
            // If an exception occurs, retrieve the ticket details again to display on the view.
            var ticket = await _service.GetTicketDetails(ticketId);
            // Return the view with the model data 
            return View(ticket);
        }
    }
    
    /// <summary>
    /// Displays the ticket completion form.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to complete.</param>
    /// <returns>An IActionResult representing the ticket completion view, a BadRequest if ticketId is null or empty, or a NotFound if the ticket is not found.</returns>
    public async Task<IActionResult> Completion(string ticketId)
    {
        // Validate the ticketId.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Completion action called with null or empty ticketId.");
            // Return 400 Bad Request
            return BadRequest("Ticket ID is required.");
        }

        // Call the service to retrieve the ticket details
        var ticket = await _service.GetTicketDetails(ticketId);
        
        // Check if the ticket exists
        if (ticket == null)
        {
            _logger.LogWarning("Ticket with ID {TicketId} not found.", ticketId);
            // Return 404 Not Found
            return NotFound();
        }

        // Call the service to retrieve help desk categories for the dropdown.
        var categories = await _categoryService.GetHelpDeskCategories();
        ViewBag.Categories = categories;

        // Create a new TicketCompletion object and populate it with data from the ticket.
        TicketCompletion completion = new TicketCompletion
        {
            TicketId = ticketId,
            Question = ticket.TicketDescription,
            Category = ticket.HelpDeskCategoryId
        };

        // Return the ticket completion view
        return View(completion);
    }
    
    /// <summary>
    /// Handles the submission of the ticket completion form.
    /// </summary>
    /// <param name="ticketCompletion">The TicketCompletion object containing the completion details.</param>
    /// <returns>An IActionResult representing the redirect to the ticket list on success, or the completion view with errors on failure.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Completion(TicketCompletion ticketCompletion)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Completion action called with null or empty ticketCompletion.");
            // Call the service to retrieve help desk categories for dropdown
            var categories = await _categoryService.GetHelpDeskCategories();
            ViewBag.Categories = categories;
            // Return the view with errors
            return View(ticketCompletion);
        }

        ticketCompletion.UserId = _user.Id;
        ticketCompletion.CompletionDate = DateTime.UtcNow;
        
        // Call the service to retrieve the ticket for updating its status.
        var ticket = await _service.GetTicketEdit(ticketCompletion.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Ticket with ID {TicketId} not found.", ticketCompletion.TicketId);
            return NotFound();
        }
        
        // Update the ticket status and modification details.
        ticket.Status = TicketStatus.Complete;
        ticket.UpdateDate = DateTime.UtcNow;
        ticket.UpdateUserId = _user.Id;

        // Call the service to create the ticket completion record and update the ticket.
        var result = await _service.CreateTicketCompletion(ticketCompletion, ticket);
        if (!result)
        {
            ModelState.AddModelError(string.Empty, "Failed to create ticket.");
            var categories = await _categoryService.GetHelpDeskCategories();
            ViewBag.Categories = categories;
            // Return the view with errors
            return View(ticketCompletion);
        }

        // Redirect to the ticket list
        return RedirectToAction(nameof(Index));
    }
    
    /// <summary>
    /// Displays the details of a ticket completion.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to view completion details for.</param>
    /// <returns>An IActionResult representing the ticket completion details view, a BadRequest if ticketId is null or empty, or a NotFound if the details are not found.</returns>
    public async Task<IActionResult> CompletionDetails(string ticketId)
    {
        // Validate the ticketId.
        if (string.IsNullOrEmpty(ticketId))
        {
            _logger.LogWarning("Completion Details action called with null or empty ticketId.");
            // Return 400 Bad Request
            return BadRequest("Ticket ID is required.");
        }

        // Call the service to retrieve the ticket completion details.
        var ticketCompletionDetails = await _service.GetTicketCompletionDetails(ticketId);

        // Check if the details exist
        if (ticketCompletionDetails == null)
        {
            _logger.LogWarning("Ticket Completion Details with ID {TicketId} not found.", ticketId);
            // Return 404 Not Found
            return NotFound();
        }
        
        // Get comment file URLs if comments exist.
        if (ticketCompletionDetails.Comments != null && ticketCompletionDetails.Comments.Any())
        {
            // Call GetCommentFileUrls method to get url
            GetCommetnFileUrls(ticketCompletionDetails.Comments);
        }

        // Return the details view
        return View(ticketCompletionDetails);
    }
    
    // <summary>
    /// Handles the creation of a new comment for a ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to add the comment to.</param>
    /// <param name="commentText">The text of the comment.</param>
    /// <param name="file">An optional file to attach to the comment.</param>
    /// <returns>An IActionResult representing a JSON response containing the newly created comment data, or a 500 Internal Server Error if an error occurs.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(string ticketId, string commentText, IFormFile? file)
    {
        try
        {
            // Get the current user's ID.
            string userId = _user.Id;
            
            // Call the service to create the comment.
            var comment = await _commentService.CreateComment(userId, ticketId, commentText.Trim(), file);

            // If a file was uploaded, generate a SAS URI for the blob.
            if (comment.FileName != null && comment.FileType != null)
            {
                comment.FileUrl = await _blobService.GetBlobSasUri(comment.FileName);
            }

            // Return the newly created comment data as a JSON response. 
            return Json(new
            {
                createDate = comment.CreateDate?.ToString("MM/dd/yyyy hh:mm:ss tt"),
                commentText = comment.CommentText,
                fileUrl = comment.FileUrl,
                fileName = comment.FileName,
                fileType = comment.FileType,
                userName = comment.AppUser?.FirstName + " " + comment.AppUser?.LastName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment.");
            // Return a 500 Internal Server Error with a message.
            // client-side AJAX error handler.
            return StatusCode(500, "An error occurred while adding the comment."); 
        }
    }

    /// <summary>
    /// Retrieves a list of users assignable to a specific ticket and category.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <param name="categoryId">The ID of the category.</param>
    /// <returns>An IActionResult representing a JSON response containing the list of assignable users, or a BadRequest if ticketId is null or empty, or a NotFound if no users are found, or a 500 Internal Server Error if an error occurs.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAssignableUsers(string ticketId, int categoryId)
    {
        // Validate the ticketId.
        if (string.IsNullOrEmpty(ticketId))
        {
            // Return 400 Bad Request
            return BadRequest("Ticket ID is required.");
        }

        try
        {
            // Call the service to get the list of assignable users.
            var users = await _assignUserService.GetAssignableUsers(ticketId, categoryId);

            // Check if any users were found.
            if (users == null || !users.Any())
            {
                // Return 404 Not Found
                return NotFound();
            }

            // Return the users as JSON
            return Json(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignable users for ticket {TicketId} and category {CategoryId}", ticketId, categoryId);
            // Return 500 Internal Server Error
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Assigns a user to a ticket.
    /// </summary>
    /// <param name="request">An AssignUserRequest object containing the UserId and TicketId.</param>
    /// <returns>An IActionResult representing a JSON response indicating success or failure, or a BadRequest if UserId or TicketId is missing, or a NotFound if the ticket or user is not found, or a 500 Internal Server Error if an error occurs.</returns>
    [HttpPost]
    [Route("Tickets/AssignUser")]
    public async Task<IActionResult> AssignUser([FromBody] AssignUserRequest request)
    {
        // Validate the request
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.TicketId))
        {
            // Return 400 Bad Request with JSON
            return BadRequest(new { success = false, message = "User ID and Ticket ID are required." });
        }

        try
        {
            // Check if the ticket exists.
            if (!await _service.IsExistingTicket(request.TicketId))
            {
                // Return 404 Not Found with JSON
                return NotFound(new { success = false, message = $"Ticket with ID '{request.TicketId}' not found." });
            }

            // Check if the user exists
            if (!await _userService.IsExistingUser(request.UserId))
            {
                // Return 404 Not Found with JSON
                return NotFound(new { success = false, message = $"User with ID '{request.UserId}' not found." });
            }

            // Create the UserTicket object
            var userTicket = new UserTicket
            {
                UserId = request.UserId,
                TicketId = request.TicketId
            };
            
            // Call the service to assign the user to the ticket
            var result = await _assignUserService.AssignUser(userTicket);

            // Check if the assignment was successful.
            if (result)
            {
                // Return 200 OK with JSON
                return Json(new { success = true });
            }
            else
            {
                // Return 500 Internal Server Error
                return StatusCode(500, "Failed to assign user to ticket.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserId} to ticket {TicketId}", request.UserId, request.TicketId);
            // Return 500 Internal Server Error
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Unassigns a user from a ticket.
    /// </summary>
    /// <param name="userId">The ID of the user to unassign.</param>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A JsonResult representing the success or failure of the operation, or a BadRequest if UserId or TicketId is missing, or a 500 Internal Server Error if an error occurs.</returns>
    [HttpDelete]
    public async Task<JsonResult> DeleteAssignedUser(string userId, string ticketId)
    {
        // Validate the input
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(ticketId))
        {
            // Return 400 Bad Request with JSON
            return Json(new { success = false, message = "User ID and Ticket ID are required." });
        }

        try
        {
            // Unassign the user from the ticket
            var result = await _assignUserService.DeleteAssignedUser(userId, ticketId);

            // Return the appropriate JSON response
            if (result)
            {
                // Return 200 OK with JSON
                return Json(new { success = true });
            }
            else
            {
                // Return 200 OK with JSON indicating failure
                return Json(new { success = false });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assigned user {UserId} from ticket {TicketId}", userId, ticketId);
            // Return 500 Internal Server Error with JSON
            return Json(new { success = false, message = "An error occurred while processing your request." });
        }
    }

    /// <summary>
    /// Creates a SelectList of TicketStatus values, excluding the "Complete" status.
    /// </summary>
    /// <returns>A SelectList of TicketStatus values, excluding "Complete".</returns>
    private SelectList GetTicketStatusSelectList()
    {
        // Get the enum select list from the HTML helper.
        var enumTicketStatus = _htmlHelper.GetEnumSelectList<TicketStatus>();
        // Filter out the "Complete" status.
        var filteredSelectList = enumTicketStatus.Where(item => item.Value != ((int)TicketStatus.Complete).ToString())
            .ToList();

        // Create and return the filtered SelectList.
        return new SelectList(filteredSelectList, "Value", "Text");
    }

    /// <summary>
    /// Generates SAS URIs for any files attached to the given comments.
    /// </summary>
    /// <param name="comments">An IEnumerable of Comment objects.</param>
    /// <returns>An IEnumerable of Comment objects with FileUrl properties populated (if applicable).</returns>
    private async Task<IEnumerable<Comment>> GetCommetnFileUrls(IEnumerable<Comment> comments)
    {
        // Iterate through the comments.
        foreach (var comment in comments)
        {
            // Check if the comment has a file attached.
            if (!string.IsNullOrEmpty(comment.FileName))
            {
                // Generate a SAS URI for the file and set it on the comment object.
                comment.FileUrl = await _blobService.GetBlobSasUri(comment.FileName);
            }
        }
        
        // Return the modified comments
        return comments;
    }
}