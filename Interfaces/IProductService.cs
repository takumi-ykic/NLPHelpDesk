using Microsoft.AspNetCore.Mvc.Rendering;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that manages products.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a list of all products.
    /// </summary>
    /// <returns>A list of Product objects.</returns>
    Task<List<Product>> GetProducts();
    
    /// <summary>
    /// Retrieves details of a specific product.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <param name="isCompleted">A flag indicating whether to include completed information.</param>
    /// <returns>A Product object representing the product details, or null if not found.</returns>
    Task<Product> GetProductDetails(string productId, bool isCompleted);
    
    /// <summary>
    /// Retrieves a product for editing.  This might involve different data or state 
    /// than viewing details (e.g., including related entities or a modified state).
    /// </summary>
    /// <param name="productId">The ID of the product to edit.</param>
    /// <returns>A Product object suitable for editing, or null if not found.</returns>
    Task<Product> GetProductEdit(string productId);
    
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">The Product object to create.</param>
    /// <returns>A boolean value indicating whether the product creation was successful.</returns>
    Task<bool> CreateProduct(Product product);
    
    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="product">The Product object to update.</param>
    /// <returns>A boolean value indicating whether the product update was successful.</returns>
    Task<bool> UpdateProduct(Product product);
    
    /// <summary>
    /// Retrieves a list of products formatted as SelectListItem objects.
    /// This is typically used for populating dropdown lists in user interfaces.
    /// </summary>
    /// <returns>A list of SelectListItem objects representing the products.</returns>
    Task<List<SelectListItem>> GetProductSelectListItems();
    
    /// <summary>
    /// Retrieves the product code associated with a product.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <returns>A ProductCode object, or null if not found.</returns>
    Task<ProductCode> GetProductCode(string productId);
    
    /// <summary>
    /// Updates the count of a product code.
    /// </summary>
    /// <param name="productId">The ID of the product for which to update the code count.</param>
    /// <returns>A boolean value indicating whether the update was successful.</returns>
    Task<bool> UpdateProductCodeCount(string productId);
}