using E_Commerce.DTOs.Requests;
using E_Commerce.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

public class ProductsController : BaseController
{
    private readonly IProductServices _productServices;

    public ProductsController(IProductServices productServices)
    {
        _productServices = productServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] FilterProductsRequest request)
    {
        var result = await _productServices.GetProducts(request);
        return ToResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var result = await _productServices.GetProductById(id);
        return ToResponse(result);
    }

    [Authorize(Roles = "Admin,SalesManager")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var result = await _productServices.CreateProduct(request);
        return ToResponse(result);
    }

    [Authorize(Roles = "Admin,SalesManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var result = await _productServices.UpdateProduct(id, request);
        return ToResponse(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productServices.DeleteProduct(id);
        return ToResponse(result);
    }
}
