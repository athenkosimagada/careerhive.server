using System.Security.Claims;
using careerhive.domain.Entities;

namespace careerhive.application.Interfaces.IService;

public interface IJwtService
{
    string GenerateAccessToken(User user, IList<string> roles);
    Task<string> GenerateRefreshTokenAsync(User user);
    Task<bool> IsRefreshTokenValidAsync(string refreshToken, User user);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
    ClaimsPrincipal GetPrincipalFromAccessToken(string accessToken);
}
