using E_Commerce.Common.Results;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;

namespace E_Commerce.Services.Users;

public interface IUserServices
{
    Task<Result<List<UserResponse>>> GetUsers();
    Task<Result<UserResponse>> GetUserById(int id);
    Task<Result<UserResponse>> EditUser(int userId, EditUserRequest request);
    Task<Result<bool>> DeleteUser(int id);
}
