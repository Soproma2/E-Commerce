using E_Commerce.Common.Results;
using E_Commerce.Common.Services;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using E_Commerce.Enums;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace E_Commerce.Services.Auth;

public class AuthServices : IAuthServices
{
    private const int EmailVerificationCodeMinutes = 10;

    private readonly DataContext _context;
    private readonly JwtService _jwtService;
    private readonly SmtpServices _smtpServices;
    private readonly ILogger<AuthServices> _logger;

    public AuthServices(DataContext context, JwtService jwtService, SmtpServices smtpServices, ILogger<AuthServices> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _smtpServices = smtpServices;
        _logger = logger;
    }

    public async Task<Result<bool>> Register(RegisterRequest request)
    {
        if (!_smtpServices.IsConfigured)
            return Result<bool>.BadRequest("Email service is not configured.");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return Result<bool>.BadRequest("Email already in use.");

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return Result<bool>.BadRequest("Username already taken.");

        if (request.Password != request.ConfirmPassword)
            return Result<bool>.BadRequest("Passwords do not match.");

        var emailVerificationCode = GenerateVerificationCode();

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            IsEmailVerified = false,
            EmailVerificationTokenHash = HashToken(emailVerificationCode),
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(EmailVerificationCodeMinutes)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        try
        {
            SendVerificationEmail(user.Email, emailVerificationCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Result<bool>.BadRequest("Verification email could not be sent.");
        }

        return Result<bool>.Success(201, true);
    }

    public async Task<Result<TokenResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<TokenResponse>.BadRequest("Invalid email or password.");

        if (!user.IsEmailVerified)
            return Result<TokenResponse>.BadRequest("Please verify your email before logging in.");

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

        if (!_smtpServices.IsConfigured)
            return Result<bool>.BadRequest("Email service is not configured.");

        var resetToken = GenerateVerificationCode();

        user.PasswordResetTokenHash = HashToken(resetToken);
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        user.UpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        try
        {
            _smtpServices.SendEmail(
                subject: "Password Reset",
                email: user.Email,
                body: $"<h2>Password Reset</h2><p>Your password reset code:</p><h1 style='letter-spacing:6px'>{resetToken}</h1><p>This code expires in 1 hour.</p>"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAt = null;
            user.UpdateAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Result<bool>.BadRequest("Password reset email could not be sent.");
        }

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return Result<bool>.NotFound("User not found.");

        var tokenHash = HashToken(request.Token);

        if (user.PasswordResetTokenHash is null ||
            user.PasswordResetTokenExpiresAt is null ||
            user.PasswordResetTokenExpiresAt < DateTime.UtcNow ||
            user.PasswordResetTokenHash != tokenHash)
        {
            return Result<bool>.BadRequest("Invalid or expired reset token.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
            return Result<bool>.BadRequest("Passwords do not match.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAt = null;
        user.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> VerifyEmail(VerifyEmailRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return Result<bool>.NotFound("User not found.");

        var tokenHash = HashToken(request.Token);

        if (user.EmailVerificationTokenHash is null ||
            user.EmailVerificationTokenExpiresAt is null ||
            user.EmailVerificationTokenExpiresAt < DateTime.UtcNow ||
            user.EmailVerificationTokenHash != tokenHash)
        {
            return Result<bool>.BadRequest("Invalid or expired verification code.");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAt = null;
        user.UpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ResendEmailVerification(ResendEmailVerificationRequest request)
    {
        if (!_smtpServices.IsConfigured)
            return Result<bool>.BadRequest("Email service is not configured.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return Result<bool>.NotFound("User not found.");

        if (user.IsEmailVerified)
            return Result<bool>.BadRequest("Email is already verified.");

        if (user.EmailVerificationTokenHash is not null &&
            user.EmailVerificationTokenExpiresAt.HasValue &&
            user.EmailVerificationTokenExpiresAt > DateTime.UtcNow)
        {
            return Result<bool>.BadRequest("Verification code is still valid. You can request a new code after it expires.");
        }

        var emailVerificationCode = GenerateVerificationCode();

        user.EmailVerificationTokenHash = HashToken(emailVerificationCode);
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(EmailVerificationCodeMinutes);
        user.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            SendVerificationEmail(user.Email, emailVerificationCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationTokenExpiresAt = null;
            user.UpdateAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Result<bool>.BadRequest("Verification email could not be sent.");
        }

        return Result<bool>.Ok(true);
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private static string GenerateVerificationCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    private void SendVerificationEmail(string email, string code)
    {
        _smtpServices.SendEmail(
            subject: "Email Verification",
            email: email,
            body: $"<h2>Email Verification</h2><p>Your verification code:</p><h1 style='letter-spacing:6px'>{code}</h1><p>This code expires in {EmailVerificationCodeMinutes} minutes.</p>"
        );
    }
}
