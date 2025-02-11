using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using NLPHelpDesk.Helpers;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Data.Models;
using static NLPHelpDesk.Helpers.Constants;

namespace NLPHelpDesk.Controllers;

/// <summary>
/// Controller for managing products.  Requires the "Technician" role.
/// </summary>
[Authorize(Roles = ROLE_TECHNICIAN)] // Make sure ROLE_TECHNICIAN is defined
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly AppUser? _user;
    private readonly UserHelper _userHelper;
    private readonly ILogger<ProductsController> _logger;
    
    /// <summary>
    /// Constructor for the ProductsController.  Injects dependencies.
    /// </summary>
    /// <param name="productService">The product service.</param>
    /// <param name="userHelper">The user helper.</param>
    /// <param name="logger">The logger.</param>
    public ProductsController(IProductService productService,
        UserHelper userHelper,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _userHelper = userHelper;
        _logger = logger;

        _user = _userHelper.GetCurrentUserAsync().Result;
    }
    
    /// <summary>
    /// Displays a list of products.
    /// </summary>
    /// <returns>An IActionResult representing the product list view.</returns>
    public async Task<IActionResult> Index()
    {
        var productList = await _productService.GetProducts();
        
        // Returns the Index view with the product list model
        return View(productList);
    }
    
    /// <summary>
    /// Displays details of a specific product.
    /// </summary>
    /// <param name="productId">The ID of the product to display.</param>
    /// <param name="isCompleted">Optional flag indicating if the ticket list is for completed or canceled. Defaults to false.</param>
    /// <returns>An IActionResult representing the product details view, or a NotFound or BadRequest result.</returns>
    public async Task<IActionResult> Details(string productId, bool isCompleted = false)
    {
        if (string.IsNullOrEmpty(productId))
        {
            _logger.LogWarning("Details action called with null or empty productId.");
            // Returns a 400 Bad Request
            return BadRequest("Product ID is required.");
        }
        
        var product = await _productService.GetProductDetails(productId, isCompleted);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found.", productId);
            // Returns a 404 Not Found
            return NotFound();
        }

        bool isOwner = product.UserId == _user.Id;

        // Create View Model
        ProductDetailsViewModel viewModel = new ProductDetailsViewModel
        {
            Product = product,
            IsOwner = isOwner,
        };
        
        // Using ViewBag for IsCompleted flag
        ViewBag.IsCompleted = isCompleted;
        // Store current URL for redirects after POST actions
        TempData[TEMP_DATA_RETURN_URL] = Request.GetDisplayUrl();
        return View(viewModel);
    }
    
    /// <summary>
    /// Displays the page for creating a new product.
    /// </summary>
    /// <returns>An IActionResult representing the product creation view.</returns>
    public IActionResult Create()
    {
        // Returns the Create view
        return View();
    }
    
    /// <summary>
    /// Handles the creation of a new product.
    /// </summary>
    /// <param name="newProduct">The Product object containing the new product's data.</param>
    /// <returns>An IActionResult representing the redirect to the Index action, or the Create view if there are errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product newProduct)
    {
        // Check if the model is valid
        if (!ModelState.IsValid)
        {
            // Return the Create view with errors if the model is invalid
            return View(newProduct);
        }

        try
        {
            // Generate a unique ProductId.
            newProduct.ProductId = Guid.NewGuid().ToString().Substring(0, 18);
            newProduct.UserId = _user?.Id;
            
            // Call the service to create the product
            var result = await _productService.CreateProduct(newProduct);

            // Check the result of the product creation
            if (!result)
            {
                // Add an error to the model state
                ModelState.AddModelError(string.Empty, "Failed to create product or product code.");
                // Return the Create view with the error message
                return View(newProduct);
            }
            
            // Redirect to the Index action (product list) after successful creation
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError("Error creating product: {Message}", ex.Message);
            // Add a general error message to the model state
            ModelState.AddModelError(string.Empty, "An error occurred while creating the product.");
            // Return the Create view with the error
            return View(newProduct);
        }
    }
    
    /// <summary>
    /// Displays the page for editing an existing product.
    /// </summary>
    /// <param name="productId">The ID of the product to edit.</param>
    /// <returns>An IActionResult representing the Edit view, or a NotFound or BadRequest result.</returns>
    public async Task<IActionResult> Edit(string productId)
    {
        if (string.IsNullOrEmpty(productId))
        {
            _logger.LogWarning("Edit action called with nul or empty productId.");
            // Return a 400 Bad Request
            return BadRequest("Product ID is required.");
        }
        
        // Call the service to get the product to edit
        var product = await _productService.GetProductEdit(productId);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found.", productId);
            // Return a 404 Not Found
            return NotFound();
        }

        // Return the Edit view with the product data
        return View(product);
    }
    
    /// <summary>
    /// Handles the updating of an existing product.
    /// </summary>
    /// <param name="updateProduct">The Product object containing the updated product data.</param>
    /// <returns>An IActionResult representing the redirect to the Details action for the updated product, or the Edit view if there are errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product updateProduct)
    {
        if (!ModelState.IsValid)
        {
            // Call the service to repopulate the product
            var product = await _productService.GetProductEdit(updateProduct.ProductId);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", updateProduct.ProductId);
                // Return a 400 Bad Request
                return NotFound();
            }
            
            ModelState.AddModelError(string.Empty, "Invalid input");
            // Return the Edit view with errors
            return View(updateProduct);
        }

        try
        {
            // Call the service to retrieve the product.
            var product = await _productService.GetProductEdit(updateProduct.ProductId);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", updateProduct.ProductId);
                // Return a 400 Bad Request
                return NotFound();
            }

            // Update only the necessary properties
            if (product.ProductName != updateProduct.ProductName)
            {
                product.ProductName = updateProduct.ProductName;
            }

            if (product.ProductDescription != updateProduct.ProductDescription)
            {
                product.ProductDescription = updateProduct.ProductDescription;
            }

            if (product.ReleaseDate != updateProduct.ReleaseDate)
            {
                product.ReleaseDate = updateProduct.ReleaseDate;
            }

            product.UpdateUserId = _user.Id;
            product.UpdateDate = DateTime.UtcNow;

            // Call the service to update the product
            var result = await _productService.UpdateProduct(product);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to update prodcut");
                // Return the Edit view with the error
                return View(updateProduct);
            }

            // Redirect to the Details view for the updated product
            return RedirectToAction(nameof(Details), new { productId = product.ProductId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while editing product {ProductId}", updateProduct.ProductId);
            // Return the Edit view with the error
            return View(updateProduct);
        }
    }
}