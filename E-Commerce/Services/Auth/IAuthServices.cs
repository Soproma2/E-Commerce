using E_Commerce.Common.Results;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;

namespace E_Commerce.Services.Auth;

public interface IAuthServices
{
    Task<Result<bool>> Register(RegisterRequest request);
    Task<Result<TokenResponse>> Login(LoginRequest request);
    Task<Result<bool>> ChangePassword(int userId, ChangePasswordRequest request);
    Task<Result<bool>> ForgotPassword(ForgotPasswordRequest request);
    Task<Result<bool>> ResetPassword(ResetPasswordRequest request);
    Task<Result<bool>> VerifyEmail(VerifyEmailRequest request);
    Task<Result<bool>> ResendEmailVerification(ResendEmailVerificationRequest request);
}
