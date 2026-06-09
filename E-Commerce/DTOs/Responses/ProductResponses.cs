using E_Commerce.Common.DTOs.Responses;
using E_Commerce.Enums;

namespace E_Commerce.DTOs.Responses;

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }
    public int Stock { get; set; }
    public string[]? Images { get; set; }
    public ProductStatus Status { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductDetailsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? EffectiveDiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string[]? Images { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

// Paged<ProductResponse> - use Common's Paged<T> directly in services
