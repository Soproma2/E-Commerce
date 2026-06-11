using E_Commerce.DTOs.Requests;
using E_Commerce.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

[Authorize]
public class AddressesController : BaseController
{
    private readonly IUserAddressServices _addressServices;

    public AddressesController(IUserAddressServices addressServices)
    {
        _addressServices = addressServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _addressServices.GetAddresses(userId);
        return ToResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddress(int id)
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _addressServices.GetAddressById(userId, id);
        return ToResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress(CreateAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _addressServices.CreateAddress(userId, request);
        return ToResponse(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, UpdateAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _addressServices.UpdateAddress(userId, id, request);
        return ToResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _addressServices.DeleteAddress(userId, id);
        return ToResponse(result);
    }

    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var userId = GetUserId();
        if (userId == 0) return InvalidUserTokenResponse();

        var result = await _addressServices.SetDefaultAddress(userId, id);
        return ToResponse(result);
    }
}
