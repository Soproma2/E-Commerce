using E_Commerce.DTOs.Requests;
using E_Commerce.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

[Authorize]
public class UsersController : BaseController
{
    private readonly IUserServices _userServices;

    public UsersController(IUserServices userServices)
    {
        _userServices = userServices;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var result = await _userServices.GetUsers();
        return ToResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _userServices.GetUserById(id);
        return ToResponse(result);
    }

    [HttpPut]
    public async Task<IActionResult> EditUser([FromBody] EditUserRequest request)
    {
        var result = await _userServices.EditUser(GetUserId(), request);
        return ToResponse(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userServices.DeleteUser(id);
        return ToResponse(result);
    }
}
