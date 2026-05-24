using E_Commerce.Common.DTOs.Requests;
using E_Commerce.Enums;

namespace E_Commerce.DTOs.Requests;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int Stock { get; set; }
    public string[]? Images { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? CategoryId { get; set; }
    public int? Stock { get; set; }
    public string[]? Images { get; set; }
    public ProductStatus? Status { get; set; }
}

public class FilterProductsRequest : PagedRequest
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ProductStatus? Status { get; set; }
}

