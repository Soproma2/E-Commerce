using E_Commerce.Common.Entities;
using E_Commerce.DTOs.Requests;
using E_Commerce.Enums;

namespace E_Commerce.Models;

public class Order : Entity
{
    public int UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
    public string? PaymentMethod { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
