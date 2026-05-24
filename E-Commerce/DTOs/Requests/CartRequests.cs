namespace E_Commerce.DTOs.Requests;

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class EditCartItemRequest
{
    public int Quantity { get; set; }
}
