using System.Security.Authentication;
using System.Security.Claims;
using AutoMapper;
using careerhive.application.DTOs;
using careerhive.application.DTOs.Request;
using careerhive.application.DTOs.Response;
using careerhive.application.Interfaces.IRepository;
using careerhive.application.Interfaces.IService;
using careerhive.domain.Entities;
using careerhive.domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace careerhive.application.Services;
public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IGenericRepository<InvalidToken> _invalidTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IUserRepository userRepository,
        IUserTokenRepository userTokenRepository,
        IGenericRepository<InvalidToken> invalidTokenRepository,
        IJwtService jwtService,
        IEmailService emailService,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<AccountService> logger)
    {
        _userRepository = userRepository;
        _userTokenRepository = userTokenRepository;
        _invalidTokenRepository = invalidTokenRepository;
        _jwtService = jwtService;
        _emailService = emailService;
        _mapper = mapper;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task RegisterAsync(RegisterRequestDto registerDto)
    {
        var existingUserWithEmail = await _userRepository.GetUserByEmailAsync(registerDto.Email);
        if (existingUserWithEmail != null)
        {
            throw new AlreadyExistsException("A user with this email already exists.");
        }

        var existingUserWithPhoneNumber = await _userRepository.GetUserAsync(u => u.PhoneNumber == registerDto.PhoneNumber);
        if (existingUserWithPhoneNumber != null)
        {
            throw new AlreadyExistsException("A user with this phone number already exists.");
        }

        var roles = (await _userRepository.GetRolesAsync()).Select(role => role!.ToLower()).ToList();

        if (!string.IsNullOrWhiteSpace(registerDto.Role) && !roles.Contains(registerDto.Role.ToLower()))
        {
            throw new NotFoundException("The requested role is invalid or unavailable.");
        }

        var user = new User
        {
            UserName = registerDto.Email.Split("@")[0],
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            FullName = $"{registerDto.FirstName} {registerDto.LastName}",
            PhoneNumber = registerDto.PhoneNumber
        };

        var createUserResult = await _userRepository.CreateAsync(user, registerDto.Password);

        if (!createUserResult.Succeeded)
        {
            var error = createUserResult.Errors.FirstOrDefault();
            throw new ArgumentException(error?.Description ?? "User registration failed.");
        }

        var roleToAssign = registerDto.Role ?? "User";
        try
        {
            var addToRoleResult = await _userRepository.AddToRoleAsync(user, roleToAssign);

            if (!addToRoleResult.Succeeded)
            {
                await _userRepository.DeleteAsync(user);
                var roleError = addToRoleResult.Errors.FirstOrDefault()?.Description ?? "Failed to assign user role.";
                throw new ArgumentException(roleError);
            }
        }
        catch (Exception ex)
        {
            await _userRepository.DeleteAsync(user);
            throw new ArgumentException("An error occurred while assigning a role.", ex);
        }
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto)
    {
        var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

        if (user == null)
        {
            _logger.LogWarning($"Login attempt with non-existent email: {loginDto.Email}");
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if(!user.EmailConfirmed)
        {
            _logger.LogWarning($"Login failed for user {loginDto.Email}. Email not verified.");
            throw new UnauthorizedAccessException("Email is not verified, please verify your email first in order to login.");
        }

        var isPasswordValid = await _userRepository.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning($"Login failed for user {loginDto.Email}: Invalid password.");
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var roles = await _userRepository.GetUserRolesAsync(user);
        
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken =  await _jwtService.GenerateRefreshTokenAsync(user);
        return new()
        {
            TokenType = "Bearer",
            AccessToken = accessToken,
            ExpiresIn = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"]!) * 60,
            RefreshToken = refreshToken,
        };
    }

    public async Task ResendConfirmationEmailAsync(ResendConfirmationRequestDto resendConfirmationRequestDto)
    {
        var user = await _userRepository.GetUserByEmailAsync(resendConfirmationRequestDto.Email);
        if (user == null)
        {
            _logger.LogWarning("Send confirmation email: User not found for email {Email}", resendConfirmationRequestDto.Email);
            throw new NotFoundException("Send confirmation email: User not found.");
        }

        if (user.EmailConfirmed)
        {
            _logger.LogWarning("Send confirmation email: Email already confirmed for user {Email}", resendConfirmationRequestDto.Email);
            throw new ArgumentException("Email is already confirmed.");
        }

        var token = await _userRepository.GenerateEmailConfirmationTokenAsync(user);

        var expiryTime = 1;
        var userToken = new ApplicationUserToken
        {
            TokenType = "EmailConfirmation",
            TokenValue = token,
            ExpiryTime = DateTime.UtcNow.AddHours(expiryTime),
            UserId = user.Id
        };

        await _userTokenRepository.AddTokenAsync(userToken);

        var confirmationLink = GenerateConfirmationLink(user.Id, token);

        var message = $@"<html><body>
            <p>Dear {user.FullName},</p>
            <p>You have requested a new email confirmation link.</p>
            <p>Please confirm your email address by clicking the link below:</p>
            <p><a href='{confirmationLink}' style='color: #007bff; text-decoration: none; font-weight: bold;'>Confirm Your Email</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <p>Regards,<br>YourWebsite Team</p>
        </body></html>";

        await _emailService.SendEmailAsync(user.Email!, "Confirm your email", message);
    }
    
    public async Task ConfirmEmailAsync(string userId, string emailToken)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(emailToken))
        {
            _logger.LogWarning("Invalid email confirmation request: Missing userId or emailToken.");
            throw new ArgumentException("Invalid email confirmation request.");
        }

        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User not found for email confirmation: {userId}");
            throw new NotFoundException("User not found.");
        }

        var token = await _userTokenRepository.GetTokenAsync(userId, "EmailConfirmation", emailToken);

        if (token == null)
        {
            _logger.LogWarning($"Email confirmation token not found in database for user: {userId}");
            throw new ArgumentException("Invalid email confirmation token.");
        }

        if (token.ExpiryTime < DateTime.UtcNow)
        {
            _logger.LogWarning($"Expired email confirmation token used for user: {userId}");
            throw new ArgumentException("Email confirmation token has expired.");
        }

        var confirmEmailResult = await _userRepository.ConfirmEmailAsync(user, emailToken);
        if (!confirmEmailResult.Succeeded)
        {
            _logger.LogError("Email confirmation failed: {Errors}", string.Join(", ", confirmEmailResult.Errors.Select(e => e.Description)));
            var firstError = confirmEmailResult.Errors.FirstOrDefault()?.Description;
            throw new ArgumentException(firstError ?? "Email confirmation failed.");
        }

        user.EmailConfirmed = true;
        var updateUserResult = await _userRepository.UpdateAsync(user);
        if(!updateUserResult.Succeeded)
        {
            _logger.LogError("User update failed: {Errors}", string.Join(", ", updateUserResult.Errors.Select(e => e.Description)));
            var firstError = confirmEmailResult.Errors.FirstOrDefault()?.Description;
            throw new ArgumentException(firstError ?? "User update failed.");
        }

        await _userTokenRepository.DeleteTokenAsync(token);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequestDto)
    {
        var user = await _userRepository.GetUserByEmailAsync(forgotPasswordRequestDto.Email);
        if (user == null)
        {
            _logger.LogWarning("Send reset password email: User not found for email {Email}", forgotPasswordRequestDto.Email);
            throw new NotFoundException("Send reset password email: User not found.");
        }

        var token = await _userRepository.GenerateResetPasswordTokenAsync(user);

        var expiryTime = 1;
        var userToken = new ApplicationUserToken
        {
            TokenType = "ResetPassword",
            TokenValue = token,
            ExpiryTime = DateTime.UtcNow.AddHours(expiryTime),
            UserId = user.Id
        };

        await _userTokenRepository.AddTokenAsync(userToken);

        var resetLink = GenerateResetPasswordLink(user.Id, token);

        var message = $@"<html><body>
                <p>Dear {user.FullName},</p>
                <p>You have requested a password reset.</p> 
                <p>Please reset your password by clicking the link below:</p>
                <p><a href='{resetLink}' style='color: #007bff; text-decoration: none; font-weight: bold;'>Reset Your Password</a></p> 
                <p>If you did not request a password reset, please ignore this email.</p> 
                <p>Regards,<br>YourWebsite Team</p>
          </body></html>";

        await _emailService.SendEmailAsync(user.Email!, "Password Reset Request", message);
    }
    
    public async Task ResetPasswordAsync(string userId, string emailToken, ResetPasswordRequestDto passwordDto)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(emailToken))
        {
            _logger.LogWarning("Invalid password reset request: Missing userId or token.");
            throw new ArgumentException("Invalid password reset request.");
        }

        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User not found for password reset: {userId}");
            throw new NotFoundException("User not found.");
        }

        var token = await _userTokenRepository.GetTokenAsync(userId, "ResetPassword", emailToken);

        if (token == null)
        {
            _logger.LogWarning($"Password reset token not found in database for user: {userId}");
            throw new ArgumentException("Invalid password reset token.");
        }

        if (token.ExpiryTime < DateTime.UtcNow)
        {
            _logger.LogWarning($"Expired password reset token used for user: {userId}");
            throw new ArgumentException("Password reset token has expired.");
        }

        var resetPasswordResult = await _userRepository.ResetPasswordAsync(user, token.TokenValue, passwordDto.Password);

        if (!resetPasswordResult.Succeeded)
        {
            _logger.LogError("Password reset failed: {Errors}", string.Join(", ", resetPasswordResult.Errors.Select(e => e.Description)));
            var firstError = resetPasswordResult.Errors.FirstOrDefault()?.Description;
            throw new ArgumentException(firstError ?? "Password reset failed.");
        }
        await _userTokenRepository.DeleteTokenAsync(token);
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto)
    {
        ClaimsPrincipal principal = _jwtService.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);

        if (principal == null)
        {
            _logger.LogWarning("Invalid access token provided for refresh token request.");
            throw new AuthenticationException("Invalid authentication credentials.");
        }

        string userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!; 

        var refreshToken = await _userTokenRepository
            .GetTokenAsync(userId, "RefreshToken", refreshTokenDto.RefreshToken);

        if (refreshToken == null || refreshToken.User == null)
        {
            _logger.LogWarning($"Refresh token not found for user {userId}: {refreshTokenDto.RefreshToken}"); // Log user ID
            throw new AuthenticationException("Invalid refresh token.");
        }

        if (refreshToken.ExpiryTime < DateTime.UtcNow)
        {
            _logger.LogWarning($"Expired refresh token used for user {userId}.");
            throw new AuthenticationException("Invalid refresh token.");
        }

        var roles = await _userRepository.GetUserRolesAsync(refreshToken.User!);

        var newAccessToken = _jwtService.GenerateAccessToken(refreshToken.User!, roles);
        var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(refreshToken.User!);

        await _userTokenRepository.DeleteTokenAsync(refreshToken);

        return new()
        {
            TokenType = "Bearer",
            AccessToken = newAccessToken,
            ExpiresIn = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"]!) * 60,
            RefreshToken = newRefreshToken,
        };
    }

    public async Task<UserInfoDto> GetUserInfoAsync(string userId)
    {
        if(!Guid.TryParse(userId, out Guid userIdGuid))
        {
            _logger.LogError($"Invalid user ID format: {userId}");
            throw new ArgumentException("Invalid user ID format.");
        }

        var user = await _userRepository.GetUserByIdAsync(userIdGuid.ToString());

        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            throw new NotFoundException($"User with ID {userId} not found.");
        }

        return _mapper.Map<UserInfoDto>(user);
    }

    public async Task UpdateUserInfoAsync(string userId, UpdateUserInfoRequestDto updateUserInfoRequestDto)
    {
        if (!Guid.TryParse(userId.ToString(), out Guid userIdGuid))
        {
            _logger.LogError($"Invalid user ID format: {userId}");
            throw new ArgumentException("Invalid user ID format.");
        }

        var user = await _userRepository.GetUserByIdAsync(userIdGuid.ToString());

        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            throw new NotFoundException($"User with ID {userId} not found.");
        }

        _mapper.Map(updateUserInfoRequestDto, user);
        user.FullName = $"{updateUserInfoRequestDto.FirstName} {updateUserInfoRequestDto.LastName}";
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userRepository.UpdateAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogError("Update user info failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            var firstError = result.Errors.FirstOrDefault()?.Description;
            throw new ArgumentException(firstError ??  "Failed to update user information.");
        }
    }

    public async Task Manage2faAsync(string userId, Manage2faRequestDto manage2FaRequestDto)
    {
        if (!Guid.TryParse(userId.ToString(), out Guid userIdGuid))
        {
            _logger.LogError($"Invalid user ID format: {userId}");
            throw new ArgumentException("Invalid user ID format.");
        }

        var user = await _userRepository.GetUserByIdAsync(userIdGuid.ToString());

        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            throw new NotFoundException($"User with ID {userId} not found.");
        }

        var result = await _userRepository.Manage2faAsync(user, manage2FaRequestDto.Enable);

        if (!result.Succeeded)
        {
            _logger.LogError("Manage 2FA failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            var firstError = result.Errors.FirstOrDefault()?.Description;
            throw new ArgumentException(firstError ?? "Failed to manage 2FA.");
        }
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequestDto changePasswordRequestDto)
    {
        if (!Guid.TryParse(userId.ToString(), out Guid userIdGuid))
        {
            _logger.LogError($"Invalid user ID format: {userId}");
            throw new ArgumentException("Invalid user ID format.");
        }

        var user = await _userRepository.GetUserByIdAsync(userIdGuid.ToString());

        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            throw new NotFoundException($"User with ID {userId} not found.");
        }

        var result = await _userRepository.ChangePasswordAsync(user, changePasswordRequestDto.OldPassword, changePasswordRequestDto.NewPassword);

        if (!result.Succeeded)
        {
            _logger.LogError("Change password failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            var firstError = result.Errors.FirstOrDefault()?.Description;
            throw new ArgumentException(firstError ?? "Failed to change password.");
        }
    }

    public async Task LogoutAsync(string userId, string accessToken)
    {
        var principal = _jwtService.GetPrincipalFromAccessToken(accessToken);
        if (principal == null)
        {
            _logger.LogWarning($"Invalid or expired token attempt for user {userId}");
            throw new UnauthorizedAccessException("Invalid or expired token.");
        }

        var expirationClaim = principal.FindFirst("exp");
        if (expirationClaim == null)
        {
            _logger.LogWarning("Expiration claim missing from the token.");
            throw new UnauthorizedAccessException("Invalid token: expiration claim missing.");
        }

        DateTime expirationTime;
        try
        {
            expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expirationClaim.Value)).DateTime;
        }
        catch (FormatException ex)
        {
            _logger.LogError($"Failed to parse expiration claim: {ex.Message}");
            throw new UnauthorizedAccessException("Invalid token: could not parse expiration claim.");
        }

        InvalidToken invalidToken = new InvalidToken
        {
            Token = accessToken,
            ExpiryTime = expirationTime,
        };

        await _invalidTokenRepository.AddAsync(invalidToken);
    }

    private string GenerateConfirmationLink(Guid userId, string token)
    {
        var encodedToken = Uri.EscapeDataString(token);
        return $"https://localhost:7264/api/auth/confirm-email?userId={userId}&token={encodedToken}";
    }

    private string GenerateResetPasswordLink(Guid userId, string token)
    {
        var encodedToken = Uri.EscapeDataString(token);
        return $"https://localhost:7264/api/auth/reset-password?userId={userId}&token={encodedToken}";
    }
}
