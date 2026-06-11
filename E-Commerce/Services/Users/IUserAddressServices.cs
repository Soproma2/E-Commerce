using E_Commerce.Common.Results;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;

namespace E_Commerce.Services.Users;

public interface IUserAddressServices
{
    Task<Result<List<AddressResponse>>> GetAddresses(int userId);
    Task<Result<AddressResponse>> GetAddressById(int userId, int addressId);
    Task<Result<AddressResponse>> CreateAddress(int userId, CreateAddressRequest request);
    Task<Result<AddressResponse>> UpdateAddress(int userId, int addressId, UpdateAddressRequest request);
    Task<Result<bool>> DeleteAddress(int userId, int addressId);
    Task<Result<bool>> SetDefaultAddress(int userId, int addressId);
}
