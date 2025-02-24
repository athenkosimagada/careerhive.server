using careerhive.application.DTOs;
using careerhive.application.DTOs.Request;
using careerhive.application.DTOs.Response;
using careerhive.domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace careerhive.application.Interfaces.IService;

public interface IAccountService
{
    Task RegisterAsync(RegisterRequestDto registerRequestDto);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);

    Task ResendConfirmationEmailAsync(ResendConfirmationRequestDto resendConfirmationRequestDto);
    Task ConfirmEmailAsync(string userId, string emailToken);

    Task ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequestDto);
    Task ResetPasswordAsync(string userId, string emailToken, ResetPasswordRequestDto resetPasswordRequestDto);
    
    Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenRequestDto);

    Task<UserInfoDto> GetUserInfoAsync(string userId);
    Task UpdateUserInfoAsync(string userId, UpdateUserInfoRequestDto updateUserInfoRequestDto);

    Task Manage2faAsync(string userId, Manage2faRequestDto manage2FaRequestDto);

    Task ChangePasswordAsync(string userId, ChangePasswordRequestDto changePasswordRequestDto);
}
