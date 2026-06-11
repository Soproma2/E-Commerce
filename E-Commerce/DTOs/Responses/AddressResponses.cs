namespace E_Commerce.DTOs.Responses;

public class AddressResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsDefault { get; set; }
}
