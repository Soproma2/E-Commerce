using E_Commerce.Common.Entities;
using E_Commerce.Enums;

namespace E_Commerce.Models;

public class User : Entity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public decimal Balance { get; set; } = 1000m;
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    // Navigation
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    public Cart? Cart { get; set; }
}
