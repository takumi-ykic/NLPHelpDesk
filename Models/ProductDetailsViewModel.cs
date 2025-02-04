namespace NLPHelpDesk.Models;

/// <summary>
/// Represents the view model for displaying product details.
/// </summary>
public class ProductDetailsViewModel
{
    public Product Product { get; set; }
    public bool IsOwner { get; set; } = false;
}