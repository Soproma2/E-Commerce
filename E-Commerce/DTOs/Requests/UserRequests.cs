namespace E_Commerce.DTOs.Requests;

public class EditUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool ClearFirstName { get; set; }
    public bool ClearLastName { get; set; }
    public bool ClearPhoneNumber { get; set; }
    public bool ClearAddress { get; set; }
}
