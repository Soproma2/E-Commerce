using E_Commerce.DTOs.Requests;
using E_Commerce.Enums;

namespace E_Commerce.DTOs.Responses;

public class OrderResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
    public string? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new List<OrderItemResponse>();
}

public class OrderItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string[]? ProductImages { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal => Quantity * Price;
}
