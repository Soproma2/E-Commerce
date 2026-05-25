using E_Commerce.DTOs.Requests;
using E_Commerce.Services.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

[Authorize]
public class CartController : BaseController
{
    private readonly ICartServices _cartServices;

    public CartController(ICartServices cartServices)
    {
        _cartServices = cartServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var result = await _cartServices.GetCart(GetUserId());
        return ToResponse(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var result = await _cartServices.AddToCart(GetUserId(), request);
        return ToResponse(result);
    }

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> EditCartItem(int cartItemId, [FromBody] EditCartItemRequest request)
    {
        var result = await _cartServices.EditCartItem(GetUserId(), cartItemId, request);
        return ToResponse(result);
    }

    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> DeleteCartItem(int cartItemId)
    {
        var result = await _cartServices.DeleteCartItem(GetUserId(), cartItemId);
        return ToResponse(result);
    }
}
