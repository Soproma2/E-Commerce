namespace E_Commerce.DTOs.Responses;

public class CartResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CartItemResponse> CartItems { get; set; } = new List<CartItemResponse>();
    public decimal TotalPrice => CartItems.Sum(x => x.Subtotal);
}

public class CartItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string[]? ProductImages { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}
