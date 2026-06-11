using E_Commerce.Common.Results;
using E_Commerce.DTOs.Responses;

namespace E_Commerce.Services.Wishlist;

public interface IWishlistServices
{
    Task<Result<List<ProductResponse>>> GetWishlist(int userId);
    Task<Result<bool>> AddToWishlist(int userId, int productId);
    Task<Result<bool>> RemoveFromWishlist(int userId, int productId);
    Task<Result<bool>> IsInWishlist(int userId, int productId);
}
