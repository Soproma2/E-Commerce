using E_Commerce.Services.Wishlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

[Authorize]
public class WishlistController : BaseController
{
    private readonly IWishlistServices _wishlistServices;

    public WishlistController(IWishlistServices wishlistServices)
    {
        _wishlistServices = wishlistServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _wishlistServices.GetWishlist(userId);
        return ToResponse(result);
    }

    [HttpPost("{productId}")]
    public async Task<IActionResult> AddToWishlist(int productId)
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _wishlistServices.AddToWishlist(userId, productId);
        return ToResponse(result);
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _wishlistServices.RemoveFromWishlist(userId, productId);
        return ToResponse(result);
    }

    [HttpGet("check/{productId}")]
    public async Task<IActionResult> IsInWishlist(int productId)
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _wishlistServices.IsInWishlist(userId, productId);
        return ToResponse(result);
    }
}
