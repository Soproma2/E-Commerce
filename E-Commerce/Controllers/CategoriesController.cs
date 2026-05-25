using E_Commerce.DTOs.Requests;
using E_Commerce.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

public class CategoriesController : BaseController
{
    private readonly ICategoryServices _categoryServices;

    public CategoriesController(ICategoryServices categoryServices)
    {
        _categoryServices = categoryServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryServices.GetCategories();
        return ToResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var result = await _categoryServices.GetCategoryById(id);
        return ToResponse(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryServices.CreateCategory(request);
        return ToResponse(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryServices.UpdateCategory(id, request);
        return ToResponse(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryServices.DeleteCategory(id);
        return ToResponse(result);
    }
}
