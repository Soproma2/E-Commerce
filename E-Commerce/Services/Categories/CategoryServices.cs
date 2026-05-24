using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Categories;

public class CategoryServices : ICategoryServices
{
    private readonly DataContext _context;

    public CategoryServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CategoryResponse>>> GetCategories()
    {
        var categories = await _context.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.ParentId == null)
            .ToListAsync();

        return Result<List<CategoryResponse>>.Ok(categories.Select(MapToResponse).ToList());
    }

    public async Task<Result<CategoryResponse>> GetCategoryById(int id)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return Result<CategoryResponse>.NotFound("Category not found.");

        return Result<CategoryResponse>.Ok(MapToResponse(category));
    }

    public async Task<Result<CategoryResponse>> CreateCategory(CreateCategoryRequest request)
    {
        if (request.ParentId.HasValue)
        {
            var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentId);
            if (!parentExists)
                return Result<CategoryResponse>.NotFound("Parent category not found.");
        }

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            Image = request.Image,
            ParentId = request.ParentId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Result<CategoryResponse>.Success(201, MapToResponse(category));
    }

    public async Task<Result<CategoryResponse>> UpdateCategory(int id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return Result<CategoryResponse>.NotFound("Category not found.");

        if (request.ParentId.HasValue)
        {
            if (request.ParentId == id)
                return Result<CategoryResponse>.BadRequest("Category cannot be its own parent.");

            var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentId);
            if (!parentExists)
                return Result<CategoryResponse>.NotFound("Parent category not found.");

            category.ParentId = request.ParentId;
        }

        if (request.Name is not null) category.Name = request.Name;
        if (request.Description is not null) category.Description = request.Description;
        if (request.Image is not null) category.Image = request.Image;

        category.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<CategoryResponse>.Ok(MapToResponse(category));
    }

    public async Task<Result<bool>> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return Result<bool>.NotFound("Category not found.");

        if (category.SubCategories.Any())
            return Result<bool>.BadRequest("Cannot delete a category that has subcategories.");

        if (category.Products.Any())
            return Result<bool>.BadRequest("Cannot delete a category that has products.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    private static CategoryResponse MapToResponse(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        Image = c.Image,
        ParentId = c.ParentId,
        ParentName = c.Parent?.Name,
        SubCategories = c.SubCategories.Select(MapToResponse).ToList()
    };
}
