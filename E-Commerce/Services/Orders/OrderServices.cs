using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using E_Commerce.Enums;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Orders;

public class OrderServices : IOrderServices
{
    private readonly DataContext _context;

    public OrderServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<OrderResponse>>> GetOrderHistory(int userId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Result<List<OrderResponse>>.Ok(orders.Select(MapToResponse).ToList());
    }

    public async Task<Result<OrderResponse>> GetOrderById(int orderId, int userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return Result<OrderResponse>.NotFound("Order not found.");

        if (order.UserId != userId)
            return Result<OrderResponse>.Unauthorized();

        return Result<OrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<Result<OrderResponse>> Checkout(int userId, CreateOrderRequest request)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || !cart.CartItems.Any())
            return Result<OrderResponse>.BadRequest("Cart is empty.");

        foreach (var item in cart.CartItems)
        {
            if (item.Product.Stock < item.Quantity)
                return Result<OrderResponse>.BadRequest($"Insufficient stock for product '{item.Product.Name}'.");
        }

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            ShippingAddress = request.ShippingAddress,
            PaymentMethod = request.PaymentMethod,
            TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price),
            OrderItems = cart.CartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Product.Price
            }).ToList()
        };

        foreach (var item in cart.CartItems)
            item.Product.Stock -= item.Quantity;

        _context.CartItems.RemoveRange(cart.CartItems);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        await _context.Entry(order).Reference(o => o.User).LoadAsync();
        foreach (var oi in order.OrderItems)
            await _context.Entry(oi).Reference(o => o.Product).LoadAsync();

        return Result<OrderResponse>.Success(201, MapToResponse(order));
    }

    public async Task<Result<OrderResponse>> UpdateOrderStatus(int orderId, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return Result<OrderResponse>.NotFound("Order not found.");

        order.Status = request.Status;
        order.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<OrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<Result<bool>> CancelOrder(int orderId, int userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return Result<bool>.NotFound("Order not found.");

        if (order.UserId != userId)
            return Result<bool>.Unauthorized();

        if (order.Status != OrderStatus.Pending)
            return Result<bool>.BadRequest("Only pending orders can be cancelled.");

        foreach (var item in order.OrderItems)
            item.Product.Stock += item.Quantity;

        order.Status = OrderStatus.Cancelled;
        order.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    private static OrderResponse MapToResponse(Order o) => new()
    {
        Id = o.Id,
        UserId = o.UserId,
        Username = o.User.Username,
        Status = o.Status,
        TotalAmount = o.TotalAmount,
        ShippingAddress = (ShippingAddressDto)o.ShippingAddress,
        PaymentMethod = o.PaymentMethod,
        CreatedAt = o.CreatedAt,
        OrderItems = o.OrderItems.Select(oi => new OrderItemResponse
        {
            Id = oi.Id,
            ProductId = oi.ProductId,
            ProductName = oi.Product.Name,
            ProductImages = oi.Product.Images,
            Quantity = oi.Quantity,
            Price = oi.Price
        }).ToList()
    };
}
