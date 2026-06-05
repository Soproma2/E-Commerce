using E_Commerce.DTOs.Requests;
using E_Commerce.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthServices _authServices;

    public AuthController(IAuthServices authServices)
    {
        _authServices = authServices;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authServices.Register(request);
        return ToResponse(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authServices.Login(request);
        return ToResponse(result);
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!TryGetUserId(out var userId))
            return InvalidUserTokenResponse();

        var result = await _authServices.ChangePassword(userId, request);
        return ToResponse(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authServices.ForgotPassword(request);
        return ToResponse(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authServices.ResetPassword(request);
        return ToResponse(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var result = await _authServices.VerifyEmail(request);
        return ToResponse(result);
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationRequest request)
    {
        var result = await _authServices.ResendEmailVerification(request);
        return ToResponse(result);
    }
}
