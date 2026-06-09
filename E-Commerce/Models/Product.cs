using E_Commerce.Common.Entities;
using E_Commerce.Enums;

namespace E_Commerce.Models;

public class Product : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int Stock { get; set; }
    public string[]? Images { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public decimal? DiscountPercent { get; set; }

    // Navigation
    public Category Category { get; set; } = null!;
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    public List<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}
