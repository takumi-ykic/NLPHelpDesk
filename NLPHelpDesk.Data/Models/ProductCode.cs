namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents a product code.
/// </summary>
public class ProductCode
{
    public string? ProductId { get; set; }
    public string? Code { get; set; }
    public int Count { get; set; } = 1;
    
    public Product? Product { get; set; }
}