namespace E_Commerce.DTOs.Responses;

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public List<CategoryResponse> SubCategories { get; set; } = new List<CategoryResponse>();
}
