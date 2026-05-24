using E_Commerce.Common.Entities;
namespace E_Commerce.Models;

public class Cart : Entity
{
    public int UserId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
}
