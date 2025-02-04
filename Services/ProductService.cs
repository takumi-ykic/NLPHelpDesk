using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Contexts;
using NLPHelpDesk.Enums;
using NLPHelpDesk.Helpers;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Models;
using Polly;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="IProductService"/> interface.
/// </summary>
public class ProductService: IProductService
{
    private readonly ApplicationContext _context;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public ProductService(ApplicationContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Retrieves a list of products that are displayed and not deleted.
    /// </summary>
    /// <returns>A list of <see cref="Product"/> objects.</returns>
    public async Task<List<Product>> GetProducts()
    {
        try
        {
            // Return product list
            return await _context.Products
                .Include(p => p.Tickets)
                .Where(p => p.Display == 1 && p.Delete == 0)
                .OrderByDescending(p => p.ReleaseDate)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving products.");
            return new List<Product>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving products.");
            return new List<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving products.");
            return new List<Product>();
        }
    }

    /// <summary>
    /// Retrieves details of a specific product, including related tickets (optionally filtered by completion status).
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <param name="isCompleted">A flag indicating whether to retrieve completed tickets. Defaults to false.</param>
    /// <returns>A <see cref="Product"/> object representing the product details, or null if not found.</returns>
    public async Task<Product> GetProductDetails(string productId, bool isCompleted = false)
    {
        // Log a warning if the productId is invalid.
        if (string.IsNullOrEmpty(productId))
        {
            _logger.LogWarning("Invalid or empty productId provided.");
            return null;
        }

        try
        {
            // Retrieve the product and related data.
            var product = await _context.Products
                .Include(p => p.Tickets)
                .ThenInclude(t => t.HelpDeskCategory)
                .Include(p => p.Tickets)
                .ThenInclude(t => t.UserTickets)
                .ThenInclude(ut => ut.AppUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            // Filter tickets by completion status if needed.
            if (product?.Tickets != null && product.Tickets.Any())
            {
                IQueryable<Ticket> ticketsQuery = product.Tickets.AsQueryable();
                ticketsQuery = isCompleted
                    ? ticketsQuery.Where(t => t.Status == TicketStatus.Complete || t.Status == TicketStatus.Canceled)
                    : ticketsQuery.Where(t => t.Status == TicketStatus.Active || t.Status == TicketStatus.Paused);

                product.Tickets = ticketsQuery.OrderByDescending(t => t.IssueDate).ToList();
            }

            // Return product
            return product;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving product details.");
            return null;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving product details.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving product details.");
            return null;
        }
    }

    // <summary>
    /// Retrieves a product for editing.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <returns>A <see cref="Product"/> object suitable for editing, or null if not found.</returns>
    public  async Task<Product> GetProductEdit(string productId)
    {
        // Log a warning if the productId is invalid.
        if (string.IsNullOrEmpty(productId))
        {
            _logger.LogWarning("Invalid or empty productId provided.");
            return null;
        }
        
        try
        {
            // Retrieve the product by ID
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving product details.");
            return null;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving product details.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving product details.");
            return null;
        }
    }

    /// <summary>
    /// Creates a new product and its associated product code.
    /// </summary>
    /// <param name="product">The <see cref="Product"/> object to create.</param>
    /// <returns>A boolean value indicating whether the product creation was successful.</returns>
    public async Task<bool> CreateProduct(Product product)
    {
        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Add the product to the database.
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Create the product code.
                var productCode = new ProductCode
                {
                    ProductId = product.ProductId,
                    Code = GenerateIdHelper.GenerateId(7).ToUpper()
                };

                // Use a retry policy to handle potential duplicate key exceptions.
                var retryPolicy = Policy
                    .Handle<DbUpdateException>(ex =>
                        ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                    .WaitAndRetryAsync(
                        retryCount: 5,
                        sleepDurationProvider: retryAttempt =>
                            TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100),
                        onRetry: (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.LogWarning(
                                $"Duplicate ProductCode generated. Retrying {retryCount}. Waiting {timeSpan.TotalMilliseconds}ms.");
                            productCode.Code = GenerateIdHelper.GenerateId(7).ToUpper();
                        });

                // Execute the retry policy to add the product code. This will retry if a DbUpdateException related to a duplicate key is thrown.
                await retryPolicy.ExecuteAsync(async () =>
                {
                    _context.ProductCodes.Add(productCode);
                    await _context.SaveChangesAsync();
                });

                // Commit the transaction if everything is successful.
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while creating a product or product code.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while creating a product or product code.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while creating a product or product code.");
                return false;
            }
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="product">The <see cref="Product"/> object containing the updated information.</param>
    /// <returns>A boolean value indicating whether the product update was successful.</returns>
    public async Task<bool> UpdateProduct(Product product)
    {
        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Update the product in the database.
                _context.Products.Update(product);

                // Save changes and check if any changes were actually made.
                if (await _context.SaveChangesAsync() > 0)
                {
                    // Commit the transaction if successful.
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("No changes detected during product update. Transaction rolled back.");
                    return false;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "A concurrency error occurred while updating the product. Another user may have modified the product.");
                return false;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while updating the product.");
                return false;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while updating the product.");
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while updating the product.");
                return false;
            }
        }
    }

    /// <summary>
    /// Retrieves a list of products as SelectListItems for use in dropdowns or other UI elements.
    /// </summary>
    /// <returns>A list of <see cref="SelectListItem"/> objects representing the products.</returns>
    public async Task<List<SelectListItem>> GetProductSelectListItems()
    {
        try
        {
            // Retrieve product list as SelectListItem
            return await _context.Products
                .Where(p => p.Display == 1 && p.Delete == 0)
                .OrderBy(p => p.ReleaseDate)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId,
                    Text = p.ProductName
                })
                .AsNoTracking()
                .ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving product select list items.");
            return new List<SelectListItem>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving product select list items.");
            return new List<SelectListItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving product select list items.");
            return new List<SelectListItem>();
        }
    }

    /// <summary>
    /// Retrieves the product code associated with a specific product.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <returns>A <see cref="ProductCode"/> object, or null if not found.</returns>
    public async Task<ProductCode> GetProductCode(string productId)
    {
        // Log a warning if the productId is invalid.
        if (string.IsNullOrEmpty(productId))
        {
            _logger.LogWarning("Invalid or empty productId provided.");
            return null;
        }

        try
        {
            // Return ProductCode list
            return await _context.ProductCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving product code.");
            return null;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving product code.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving product code.");
            return null;
        }
    }

    /// <summary>
    /// Updates the count of a product code.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <returns>A boolean value indicating whether the update was successful.</returns>
    public async Task<bool> UpdateProductCodeCount(string productId)
    {
        try
        {
            // Retrieve the product code.
            var productCode = await _context.ProductCodes
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (productCode != null)
            {
                // Increment the count.
                productCode.Count += 1;
            }
            else
            {
                _logger.LogWarning($"Product code with ID {productId} not found");
                return false;
            }

            // Save changes and check if any changes were actually made.
            var saveResult = await _context.SaveChangesAsync();

            if (saveResult > 0)
            {
                return true;
            }
            else
            {
                _logger.LogWarning("No changes detected during product code count update.");
                return false;
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex,
                "A concurrency error occurred while updating the product code. Another user may have modified the product code.");
            return false;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while updating the product code.");
            return false;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while updating the product code.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating the product code.");
            return false;
        }
    }
}