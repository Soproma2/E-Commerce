using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Users;

public class UserServices : IUserServices
{
    private readonly DataContext _context;

    public UserServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserResponse>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Result<List<UserResponse>>.Ok(users.Select(MapToResponse).ToList());
    }

    public async Task<Result<UserResponse>> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
            return Result<UserResponse>.NotFound("User not found.");

        return Result<UserResponse>.Ok(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> EditUser(int userId, EditUserRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
            return Result<UserResponse>.NotFound("User not found.");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;
        if (request.Address is not null) user.Address = request.Address;

        user.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<UserResponse>.Ok(MapToResponse(user));
    }

    public async Task<Result<bool>> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
            return Result<bool>.NotFound("User not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    private static UserResponse MapToResponse(E_Commerce.Models.User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        Role = u.Role,
        FirstName = u.FirstName,
        LastName = u.LastName,
        PhoneNumber = u.PhoneNumber,
        Address = u.Address,
        CreatedAt = u.CreatedAt
    };
}
