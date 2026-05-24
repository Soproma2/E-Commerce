using E_Commerce.Enums;

namespace E_Commerce.DTOs.Requests;

public class CreateOrderRequest
{
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
    public string? PaymentMethod { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class ShippingAddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
