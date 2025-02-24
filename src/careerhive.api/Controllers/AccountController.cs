using System.Security.Claims;
using Azure.Core;
using careerhive.application.DTOs;
using careerhive.application.DTOs.Request;
using careerhive.application.DTOs.Response;
using careerhive.application.Interfaces.IService;
using careerhive.domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace careerhive.api.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService authService)
    {
        _accountService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto registerDto)
    {
        try
        {
            await _accountService.RegisterAsync(registerDto);
            return Ok(new 
            { 
                Success = true, 
                StatusCode = StatusCodes.Status200OK, 
                Message = "User registered successfully" 
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto loginDto)
    {
        try
        {
            var loginResponseDto = await _accountService.LoginAsync(loginDto);

            return Ok(new 
            { 
                Success = true, 
                StatusCode = StatusCodes.Status200OK, 
                Token = loginResponseDto
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto refreshTokenDto)
    {
        try
        {
            var refreshTokenResponseDto = await _accountService.RefreshTokenAsync(refreshTokenDto);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Token = refreshTokenResponseDto
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("resendConfirmationEmail")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequestDto resendConfirmationDto)
    {
        try
        {
            await _accountService.ResendConfirmationEmailAsync(resendConfirmationDto);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully sent confirmation link to the provide email address."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpGet("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        try
        {
            await _accountService.ConfirmEmailAsync(userId, Uri.UnescapeDataString(token));

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = $"Successfully confirmed your email address for your account."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto forgotPasswordRequestDto)
    {
        try
        {
            await _accountService.ForgotPasswordAsync(forgotPasswordRequestDto);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Password reset link sent successfully. Please check your email."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordRequestDto, [FromQuery] string userId, [FromQuery] string token)
    {
        try
        {
            await _accountService.ResetPasswordAsync(userId, Uri.UnescapeDataString(token), resetPasswordRequestDto);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully reset password for your account."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpGet("manage/info")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token."
                });
            }

            var userInfoDto = await _accountService.GetUserInfoAsync(userId);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                UserInfo = userInfoDto
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("manage/info")]
    [Authorize]
    public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserInfoRequestDto updateUserInfoRequestDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token."
                });
            }

            await _accountService.UpdateUserInfoAsync(userId, updateUserInfoRequestDto); 

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "User information updated successfully."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("manage/2fa")]
    [Authorize]
    public async Task<IActionResult> Manage2fa(Manage2faRequestDto manage2FaRequestDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token."
                });
            }

            await _accountService.Manage2faAsync(userId, manage2FaRequestDto);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "2FA settings updated successfully."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("manage/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto changePasswordRequestDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token."
                });
            }

            await _accountService.ChangePasswordAsync(userId, changePasswordRequestDto);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Password changed successfully."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }
}
