using E_Commerce.Common.Results;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;

namespace E_Commerce.Services.Orders;

public interface IOrderServices
{
    Task<Result<List<OrderResponse>>> GetOrderHistory(int userId);
    Task<Result<OrderResponse>> GetOrderById(int orderId, int userId);
    Task<Result<OrderResponse>> Checkout(int userId, CreateOrderRequest request);
    Task<Result<OrderResponse>> UpdateOrderStatus(int orderId, UpdateOrderStatusRequest request);
    Task<Result<bool>> CancelOrder(int orderId, int userId);
}
