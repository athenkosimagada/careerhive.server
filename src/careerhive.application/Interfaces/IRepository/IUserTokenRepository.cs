using careerhive.domain.Entities;

namespace careerhive.application.Interfaces.IRepository;
public interface IUserTokenRepository
{
    Task<ApplicationUserToken?> GetTokenAsync(string userId, string tokenType, string tokenValue);
    Task AddTokenAsync(ApplicationUserToken token);
    Task UpdateTokenAsync(ApplicationUserToken token);
    Task DeleteTokenAsync(ApplicationUserToken token);
    Task DeleteExpiredTokensAsync(string tokenType);
    Task<List<ApplicationUserToken>?> GetUserTokensAsync(Guid userId, string tokenType);
}
