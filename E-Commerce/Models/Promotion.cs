using E_Commerce.Common.Entities;

namespace E_Commerce.Models;

public class Promotion : Entity
{
    public string Name { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? ProductId { get; set; }
    public int? CategoryId { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Navigation
    public Product? Product { get; set; }
    public Category? Category { get; set; }
}
