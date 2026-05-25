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
        if (!TryGetUserId(out var userId))
            return InvalidUserTokenResponse();

        var result = await _cartServices.GetCart(userId);
        return ToResponse(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (!TryGetUserId(out var userId))
            return InvalidUserTokenResponse();

        var result = await _cartServices.AddToCart(userId, request);
        return ToResponse(result);
    }

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> EditCartItem(int cartItemId, [FromBody] EditCartItemRequest request)
    {
        if (!TryGetUserId(out var userId))
            return InvalidUserTokenResponse();

        var result = await _cartServices.EditCartItem(userId, cartItemId, request);
        return ToResponse(result);
    }

    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> DeleteCartItem(int cartItemId)
    {
        if (!TryGetUserId(out var userId))
            return InvalidUserTokenResponse();

        var result = await _cartServices.DeleteCartItem(userId, cartItemId);
        return ToResponse(result);
    }
}
