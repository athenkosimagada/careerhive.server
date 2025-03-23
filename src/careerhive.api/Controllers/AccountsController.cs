using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Azure.Core;
using careerhive.application.Interfaces.IRepository;
using careerhive.application.Interfaces.IService;
using careerhive.application.Request;
using careerhive.domain.Entities;
using careerhive.domain.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace careerhive.api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountsService _accountsService;
    private readonly IGenericRepository<UserSubscription> _userSubscriptionRepository;
    private readonly IGenericRepository<InvalidToken> _invalidTokenRepository;

    public AccountsController(IAccountsService authService, 
        IGenericRepository<UserSubscription> userSubscriptionRepository,
        IGenericRepository<InvalidToken> invalidTokenRepository)
    {
        _accountsService = authService;
        _userSubscriptionRepository = userSubscriptionRepository;
        _invalidTokenRepository = invalidTokenRepository;
    }

    [HttpPost("register")]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> Register(RegisterRequestDto registerDto)
    {
        try
        {
            await _accountsService.RegisterAsync(registerDto);
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
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequestDto loginDto)
    {
        try
        {
            var loginResponseDto = await _accountsService.LoginAsync(loginDto);

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
    [EnableRateLimiting("post")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto refreshTokenDto)
    {
        try
        {
            var refreshTokenResponseDto = await _accountsService.RefreshTokenAsync(refreshTokenDto);

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
    [EnableRateLimiting("post")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequestDto resendConfirmationDto)
    {
        try
        {
            await _accountsService.ResendConfirmationEmailAsync(resendConfirmationDto);

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
    [EnableRateLimiting("get")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        try
        {
            await _accountsService.ConfirmEmailAsync(userId, Uri.UnescapeDataString(token));

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
    [EnableRateLimiting("post")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto forgotPasswordRequestDto)
    {
        try
        {
            await _accountsService.ForgotPasswordAsync(forgotPasswordRequestDto);

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
    [EnableRateLimiting("post")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordRequestDto, [FromQuery] string userId, [FromQuery] string token)
    {
        try
        {
            await _accountsService.ResetPasswordAsync(userId, Uri.UnescapeDataString(token), resetPasswordRequestDto);

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
    [EnableRateLimiting("get")]
    public async Task<IActionResult> GetUserInfo()
    {
        try
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t =>  t.Token == accessToken);
            if (isTokenInvalid)
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token.."
                });
            }

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

            var userInfoResponseDto = await _accountsService.GetUserInfoAsync(userId);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                UserInfo = userInfoResponseDto
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("manage/info")]
    [Authorize]
    [EnableRateLimiting("post")]
    public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserInfoRequestDto updateUserInfoRequestDto)
    {
        try
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
            if (isTokenInvalid)
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token.."
                });
            }

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

            await _accountsService.UpdateUserInfoAsync(userId, updateUserInfoRequestDto); 

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
    [EnableRateLimiting("post")]
    public async Task<IActionResult> Manage2fa(Manage2faRequestDto manage2FaRequestDto)
    {
        try
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
            if (isTokenInvalid)
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token.."
                });
            }

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

            await _accountsService.Manage2faAsync(userId, manage2FaRequestDto);

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
    [EnableRateLimiting("post")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto changePasswordRequestDto)
    {
        try
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
            if (isTokenInvalid)
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token.."
                });
            }

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

            await _accountsService.ChangePasswordAsync(userId, changePasswordRequestDto);

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

    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("post")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
            if (isTokenInvalid)
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token.."
                });
            }

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

            await _accountsService.LogoutAsync(userId, accessToken);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "User logged out successfully."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("subscribe")]
    [Authorize]
    [EnableRateLimiting("post")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequestDto subscribeRequestDto)
    {
        try
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
            if (isTokenInvalid)
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token.."
                });
            }

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

            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                return BadRequest(new
                {
                    Success = true,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid job ID format."
                });
            }

            var existingUserSubscription = await _userSubscriptionRepository.FirstOrDefaultAsync(u => u.UserId == userIdGuid);
            if (existingUserSubscription != null)
            {
                existingUserSubscription.IsActive = true;
                await _userSubscriptionRepository.UpdateAsync(existingUserSubscription);

                return Ok(new
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Subscription activated successfully."
                });
            }

            var userSubscription = new UserSubscription
            {
                UserId = userIdGuid,
                Email = subscribeRequestDto.Email,
                IsActive = true,
            };

            await _userSubscriptionRepository.AddAsync(userSubscription);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Subscribed successfully."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("unsubscribe")]
    [Authorize]
    [EnableRateLimiting("post")]
    public async Task<IActionResult> UnSubscribe([FromBody] UnSubscribeRequestDto unSubscribeRequestDto)
    {
        try
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            bool isTokenInvalid = await _invalidTokenRepository.ExistsAsync(t => t.Token == accessToken);
            if (isTokenInvalid)
            {
                return Unauthorized(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid authentication token.."
                });
            }

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

            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                return BadRequest(new
                {
                    Success = true,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid job ID format."
                });
            }

            var existingUserSubscription = await _userSubscriptionRepository.FirstOrDefaultAsync(u => u.UserId == userIdGuid);
            if (existingUserSubscription == null)
            {
                return NotFound(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Subscription not found."
                });
            }

            existingUserSubscription.IsActive = false;
            await _userSubscriptionRepository.UpdateAsync(existingUserSubscription);

            return Ok(new
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Unsubscribed successfully."
            });
        }
        catch (Exception)
        {
            throw;
        }
    }
}
