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
        var allCategories = await _context.Categories.ToListAsync();

        var roots = allCategories
            .Where(c => c.ParentId == null)
            .Select(c => BuildTree(c, allCategories))
            .ToList();

        return Result<List<CategoryResponse>>.Ok(roots);
    }

    public async Task<Result<CategoryResponse>> GetCategoryById(int id)
    {
        var allCategories = await _context.Categories.ToListAsync();
        var category = allCategories.FirstOrDefault(c => c.Id == id);

        if (category is null)
            return Result<CategoryResponse>.NotFound("Category not found.");

        category.Parent = allCategories.FirstOrDefault(c => c.Id == category.ParentId);

        return Result<CategoryResponse>.Ok(BuildTree(category, allCategories));
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
            ParentId = request.ParentId,
            DiscountPercent = request.DiscountPercent
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

        if (request.ClearParent)
        {
            category.ParentId = null;
        }
        else if (request.ParentId.HasValue)
        {
            if (request.ParentId == id)
                return Result<CategoryResponse>.BadRequest("Category cannot be its own parent.");

            var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentId);
            if (!parentExists)
                return Result<CategoryResponse>.NotFound("Parent category not found.");

            if (await WouldCreateCycle(id, request.ParentId.Value))
                return Result<CategoryResponse>.BadRequest("Category cannot use one of its subcategories as parent.");

            category.ParentId = request.ParentId;
        }

        if (request.Name is not null) category.Name = request.Name;
        if (request.ClearDescription) category.Description = null;
        else if (request.Description is not null) category.Description = request.Description;
        if (request.ClearImage) category.Image = null;
        else if (request.Image is not null) category.Image = request.Image;
        if (request.ClearDiscount) category.DiscountPercent = null;
        else if (request.DiscountPercent.HasValue) category.DiscountPercent = request.DiscountPercent;

        category.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<CategoryResponse>.Ok(MapToResponse(category));
    }

    public async Task<Result<CategoryResponse>> UpdateCategoryDiscount(int id, UpdateCategoryDiscountRequest request)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return Result<CategoryResponse>.NotFound("Category not found.");

        if (request.ClearDiscount)
            category.DiscountPercent = null;
        else if (request.DiscountPercent.HasValue)
            category.DiscountPercent = request.DiscountPercent;

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
        DiscountPercent = c.DiscountPercent,
        SubCategories = c.SubCategories.Select(MapToResponse).ToList()
    };

    private static CategoryResponse BuildTree(Category category, List<Category> allCategories)
    {
        var children = allCategories
            .Where(c => c.ParentId == category.Id)
            .ToList();

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Image = category.Image,
            ParentId = category.ParentId,
            ParentName = category.Parent?.Name,
            DiscountPercent = category.DiscountPercent,
            SubCategories = children.Select(c => BuildTree(c, allCategories)).ToList()
        };
    }

    private async Task<bool> WouldCreateCycle(int categoryId, int parentId)
    {
        var currentParentId = parentId;

        while (true)
        {
            if (currentParentId == categoryId)
                return true;

            var nextParentId = await _context.Categories
                .Where(c => c.Id == currentParentId)
                .Select(c => c.ParentId)
                .FirstOrDefaultAsync();

            if (!nextParentId.HasValue)
                return false;

            currentParentId = nextParentId.Value;
        }
    }
}
