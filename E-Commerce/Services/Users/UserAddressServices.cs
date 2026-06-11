using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Users;

public class UserAddressServices : IUserAddressServices
{
    private readonly DataContext _context;

    public UserAddressServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AddressResponse>>> GetAddresses(int userId)
    {
        var addresses = await _context.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => MapToResponse(a))
            .ToListAsync();

        return Result<List<AddressResponse>>.Ok(addresses);
    }

    public async Task<Result<AddressResponse>> GetAddressById(int userId, int addressId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId);

        if (address is null)
            return Result<AddressResponse>.NotFound("Address not found.");

        return Result<AddressResponse>.Ok(MapToResponse(address));
    }

    public async Task<Result<AddressResponse>> CreateAddress(int userId, CreateAddressRequest request)
    {
        if (request.IsDefault)
        {
            await ResetDefaultAddress(userId);
        }

        var address = new UserAddress
        {
            UserId = userId,
            FullName = request.FullName,
            Street = request.Street,
            City = request.City,
            Country = request.Country,
            ZipCode = request.ZipCode,
            PhoneNumber = request.PhoneNumber,
            IsDefault = request.IsDefault
        };

        _context.UserAddresses.Add(address);
        await _context.SaveChangesAsync();

        return Result<AddressResponse>.Ok(MapToResponse(address));
    }

    public async Task<Result<AddressResponse>> UpdateAddress(int userId, int addressId, UpdateAddressRequest request)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId);

        if (address is null)
            return Result<AddressResponse>.NotFound("Address not found.");

        if (request.IsDefault && !address.IsDefault)
        {
            await ResetDefaultAddress(userId);
        }

        address.FullName = request.FullName;
        address.Street = request.Street;
        address.City = request.City;
        address.Country = request.Country;
        address.ZipCode = request.ZipCode;
        address.PhoneNumber = request.PhoneNumber;
        address.IsDefault = request.IsDefault;
        address.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<AddressResponse>.Ok(MapToResponse(address));
    }

    public async Task<Result<bool>> DeleteAddress(int userId, int addressId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId);

        if (address is null)
            return Result<bool>.NotFound("Address not found.");

        _context.UserAddresses.Remove(address);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> SetDefaultAddress(int userId, int addressId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId);

        if (address is null)
            return Result<bool>.NotFound("Address not found.");

        await ResetDefaultAddress(userId);

        address.IsDefault = true;
        address.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    private async Task ResetDefaultAddress(int userId)
    {
        var defaultAddress = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

        if (defaultAddress != null)
        {
            defaultAddress.IsDefault = false;
            defaultAddress.UpdateAt = DateTime.UtcNow;
        }
    }

    private static AddressResponse MapToResponse(UserAddress a)
    {
        return new AddressResponse
        {
            Id = a.Id,
            FullName = a.FullName,
            Street = a.Street,
            City = a.City,
            Country = a.Country,
            ZipCode = a.ZipCode,
            PhoneNumber = a.PhoneNumber,
            IsDefault = a.IsDefault
        };
    }
}
