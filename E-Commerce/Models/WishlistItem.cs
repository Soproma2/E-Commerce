using E_Commerce.Common.Entities;

namespace E_Commerce.Models;

public class WishlistItem : Entity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
