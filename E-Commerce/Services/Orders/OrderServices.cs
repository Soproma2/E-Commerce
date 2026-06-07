using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using E_Commerce.Enums;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace E_Commerce.Services.Orders;

public class OrderServices : IOrderServices
{
    private readonly DataContext _context;

    public OrderServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<OrderResponse>>> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Result<List<OrderResponse>>.Ok(orders.Select(MapToResponse).ToList());
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
            return Result<OrderResponse>.Forbidden();

        return Result<OrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<Result<OrderResponse>> Checkout(int userId, CreateOrderRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || !cart.CartItems.Any())
            return Result<OrderResponse>.BadRequest("Cart is empty.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Result<OrderResponse>.NotFound("User not found.");

        foreach (var item in cart.CartItems)
        {
            if (item.Product.Status != ProductStatus.Active)
                return Result<OrderResponse>.BadRequest($"Product '{item.Product.Name}' is not available.");

            if (item.Product.Stock < item.Quantity)
                return Result<OrderResponse>.BadRequest($"Insufficient stock for product '{item.Product.Name}'.");
        }

        var totalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

        if (user.Balance < totalAmount)
            return Result<OrderResponse>.BadRequest("Insufficient balance.");

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            ShippingAddress = request.ShippingAddress,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Test balance" : request.PaymentMethod,
            TotalAmount = totalAmount,
            OrderItems = cart.CartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Product.Price
            }).ToList()
        };

        foreach (var item in cart.CartItems)
        {
            var updatedRows = await _context.Products
                .Where(p => p.Id == item.ProductId &&
                            p.Status == ProductStatus.Active &&
                            p.Stock >= item.Quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Stock, p => p.Stock - item.Quantity));

            if (updatedRows == 0)
                return Result<OrderResponse>.BadRequest($"Insufficient stock for product '{item.Product.Name}'.");
        }

        user.Balance -= totalAmount;
        user.UpdateAt = DateTime.UtcNow;

        _context.CartItems.RemoveRange(cart.CartItems);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _context.Entry(order).Reference(o => o.User).LoadAsync();
        foreach (var oi in order.OrderItems)
            await _context.Entry(oi).Reference(o => o.Product).LoadAsync();

        return Result<OrderResponse>.Success(201, MapToResponse(order));
    }

    public async Task<Result<OrderResponse>> UpdateOrderStatus(int orderId, UpdateOrderStatusRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return Result<OrderResponse>.NotFound("Order not found.");

        if (!IsValidStatusTransition(order.Status, request.Status))
            return Result<OrderResponse>.BadRequest($"Cannot change order status from {order.Status} to {request.Status}.");

        if (request.Status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            foreach (var item in order.OrderItems)
            {
                await _context.Products
                    .Where(p => p.Id == item.ProductId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.Stock, p => p.Stock + item.Quantity));
            }

            order.User.Balance += order.TotalAmount;
            order.User.UpdateAt = DateTime.UtcNow;
        }

        order.Status = request.Status;
        order.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result<OrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<Result<bool>> CancelOrder(int orderId, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return Result<bool>.NotFound("Order not found.");

        if (order.UserId != userId)
            return Result<bool>.Forbidden();

        if (order.Status != OrderStatus.Pending)
            return Result<bool>.BadRequest("Only pending orders can be cancelled.");

        foreach (var item in order.OrderItems)
        {
            await _context.Products
                .Where(p => p.Id == item.ProductId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Stock, p => p.Stock + item.Quantity));
        }

        var user = await _context.Users.FindAsync(userId);

        if (user is null)
            return Result<bool>.NotFound("User not found.");

        user.Balance += order.TotalAmount;
        user.UpdateAt = DateTime.UtcNow;

        order.Status = OrderStatus.Cancelled;
        order.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result<bool>.Ok(true);
    }

    private static OrderResponse MapToResponse(Order o) => new()
    {
        Id = o.Id,
        UserId = o.UserId,
        Username = o.User.Username,
        Status = o.Status,
        TotalAmount = o.TotalAmount,
        ShippingAddress = o.ShippingAddress,
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

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus nextStatus)
    {
        if (currentStatus == nextStatus)
            return true;

        return currentStatus switch
        {
            OrderStatus.Pending => nextStatus is OrderStatus.Paid or OrderStatus.Cancelled,
            OrderStatus.Paid => nextStatus is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => nextStatus == OrderStatus.Delivered,
            _ => false
        };
    }
}
