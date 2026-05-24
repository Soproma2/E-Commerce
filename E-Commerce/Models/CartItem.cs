using E_Commerce.Common.Entities;
namespace E_Commerce.Models;

public class CartItem : Entity
{
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    // Navigation
    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
