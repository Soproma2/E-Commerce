using E_Commerce.Common.Entities;

namespace E_Commerce.Models;

public class ProductReview : Entity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
