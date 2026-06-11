using E_Commerce.Common.Entities;

namespace E_Commerce.Models;

public class UserAddress : Entity
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsDefault { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
