using E_Commerce.Common.Entities;
namespace E_Commerce.Models;

public class Category : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int? ParentId { get; set; }

    // Navigation
    public Category? Parent { get; set; }
    public List<Category> SubCategories { get; set; } = new List<Category>();
    public List<Product> Products { get; set; } = new List<Product>();
}
