using E_Commerce.Common.Results;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;

namespace E_Commerce.Services.Categories;

public interface ICategoryServices
{
    Task<Result<List<CategoryResponse>>> GetCategories();
    Task<Result<CategoryResponse>> GetCategoryById(int id);
    Task<Result<CategoryResponse>> CreateCategory(CreateCategoryRequest request);
    Task<Result<CategoryResponse>> UpdateCategory(int id, UpdateCategoryRequest request);
    Task<Result<CategoryResponse>> UpdateCategoryDiscount(int id, UpdateCategoryDiscountRequest request);
    Task<Result<bool>> DeleteCategory(int id);
}
