namespace E_Commerce.DTOs.Requests;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int? ParentId { get; set; }
    public decimal? DiscountPercent { get; set; }
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int? ParentId { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool ClearDescription { get; set; }
    public bool ClearImage { get; set; }
    public bool ClearParent { get; set; }
    public bool ClearDiscount { get; set; }
}

public class UpdateCategoryDiscountRequest
{
    public decimal? DiscountPercent { get; set; }
    public bool ClearDiscount { get; set; }
}
