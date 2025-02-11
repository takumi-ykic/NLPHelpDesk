using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents a product.
/// </summary>
public class Product
{
    [StringLength(20)]
    [DisplayName("Product ID")]
    public string? ProductId { get; set; }
    [StringLength(60, MinimumLength = 6)]
    [DisplayName("Product Name")]
    public string? ProductName { get; set; }
    [StringLength(400)]
    [DisplayName("Product Description")]
    public string? ProductDescription { get; set; }
    public string? UserId { get; set; }
    [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
    [DisplayName("Release Date")]
    public DateTime? ReleaseDate { get; set; }
    public string? UpdateUserId { get; set; }
    [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
    [DisplayName("Update Date")]
    public DateTime? UpdateDate { get; set; }

    public int Display { get; set; } = 1;
    public int Delete { get; set; } = 0;
    
    public ProductCode? ProductCode { get; set; }
    public AppUser? CreateUser { get; set; }
    public AppUser? UpdateUser { get; set; }
    public ICollection<Ticket>? Tickets { get; set; }
}