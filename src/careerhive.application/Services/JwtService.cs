using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using careerhive.application.Interfaces.IRepository;
using careerhive.application.Interfaces.IService;
using careerhive.domain.Entities;
using careerhive.domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace careerhive.application.Services;
public class JwtService : IJwtService
{
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(
        IUserTokenRepository userTokenRepository,
        IConfiguration configuration, 
        ILogger<JwtService> logger)
    {
        _userTokenRepository = userTokenRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateAccessToken(User user, IList<string> roles)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"];
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];
        var accessTonkeExpirationMinutes = double.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"]!);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Aud, audience!)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        claims.Add(new Claim("FirstName", user.FirstName ?? ""));
        claims.Add(new Claim("LastName", user.LastName ?? ""));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(accessTonkeExpirationMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(User user)
    {
        var refreshTokenExpirationDays = double.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]!);

        var refreshToken = GenerateRefreshTokenString();

        var applicationUserToken = new ApplicationUserToken
        {
            UserId = user.Id,
            TokenType = "RefreshToken",
            TokenValue = refreshToken,
            ExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            UsedTime = DateTime.UtcNow,
            User = user,
        };


        await _userTokenRepository.AddTokenAsync(applicationUserToken);

        return refreshToken;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"];
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,

            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;

        if (!tokenHandler.CanReadToken(accessToken))
        {
            _logger.LogWarning("Invalid token format. Token cannot be read as a JWT.");
            throw new SecurityTokenException("Invalid token provided.");
        }

        try
        {
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out securityToken);
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "Expired token used for refresh token request.");
            throw new SecurityTokenExpiredException("The access token has expired.", ex);
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogError(ex, "Invalid token signature.");
            throw new SecurityTokenInvalidSignatureException("The access token has an invalid signature.", ex);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "A SecurityTokenException occurred: {Message}", ex.Message);
            throw new SecurityTokenException("An error occurred during token validation.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during token validation.");
            throw new SecurityTokenException("Token validation failed due to an unexpected error.", ex);
        }
    }

    public async Task<bool> IsRefreshTokenValidAsync(string refreshToken, User user)
    {
        var token = await _userTokenRepository.GetTokenAsync(user.Id.ToString(), "RefreshToken", refreshToken);

        if (token == null) return false;

        return token.TokenValue == refreshToken;
    }

    private string GenerateRefreshTokenString()
    {
        var randomNumberGenerator = RandomNumberGenerator.Create();
        var bytes = new byte[64];
        randomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
