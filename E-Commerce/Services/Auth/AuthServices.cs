using E_Commerce.Common.Results;
using E_Commerce.Common.Services;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using E_Commerce.Enums;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Auth;

public class AuthServices : IAuthServices
{
    private readonly DataContext _context;
    private readonly JwtService _jwtService;
    private readonly SmtpServices _smtpServices;

    public AuthServices(DataContext context, JwtService jwtService, SmtpServices smtpServices)
    {
        _context = context;
        _jwtService = jwtService;
        _smtpServices = smtpServices;
    }

    public async Task<Result<TokenResponse>> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return Result<TokenResponse>.BadRequest("Email already in use.");

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return Result<TokenResponse>.BadRequest("Username already taken.");

        if (request.Password != request.ConfirmPassword)
            return Result<TokenResponse>.BadRequest("Passwords do not match.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateJwtToken(user);

        return Result<TokenResponse>.Success(201, new TokenResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        });
    }

    public async Task<Result<TokenResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<TokenResponse>.BadRequest("Invalid email or password.");

        var token = _jwtService.GenerateJwtToken(user);

        return Result<TokenResponse>.Ok(new TokenResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        });
    }

    public async Task<Result<bool>> ChangePassword(int userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
            return Result<bool>.NotFound("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return Result<bool>.BadRequest("Current password is incorrect.");

        if (request.NewPassword != request.ConfirmNewPassword)
            return Result<bool>.BadRequest("Passwords do not match.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return Result<bool>.NotFound("User not found.");

        var resetToken = Guid.NewGuid().ToString();

        // TODO: store reset token with expiry in DB
        _smtpServices.SendEmail(
            subject: "Password Reset",
            email: user.Email,
            body: $"Your password reset token: {resetToken}"
        );

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return Result<bool>.NotFound("User not found.");

        // TODO: validate token from DB

        if (request.NewPassword != request.ConfirmNewPassword)
            return Result<bool>.BadRequest("Passwords do not match.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> VerifyEmail(VerifyEmailRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return Result<bool>.NotFound("User not found.");

        // TODO: validate token from DB

        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }
}
