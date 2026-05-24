using E_Commerce.Common.Results;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;

namespace E_Commerce.Services.Cart;

public interface ICartServices
{
    Task<Result<CartResponse>> GetCart(int userId);
    Task<Result<CartResponse>> AddToCart(int userId, AddToCartRequest request);
    Task<Result<CartResponse>> EditCartItem(int userId, int cartItemId, EditCartItemRequest request);
    Task<Result<bool>> DeleteCartItem(int userId, int cartItemId);
}
