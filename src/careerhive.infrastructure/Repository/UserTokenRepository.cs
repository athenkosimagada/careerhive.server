using careerhive.application.Interfaces.IRepository;
using careerhive.domain.Entities;
using careerhive.infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace careerhive.infrastructure.Repository;
public class UserTokenRepository : IUserTokenRepository
{
    private readonly ApplicationDbContext _context;

    public UserTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddTokenAsync(ApplicationUserToken token)
    {
        await _context.ApplicationUserTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredTokensAsync(string tokenType)
    {
        var expiredTokens = await _context.ApplicationUserTokens
            .Where(t => t.TokenType == tokenType && t.ExpiryTime != null && t.ExpiryTime <= DateTime.UtcNow)
            .ToListAsync();

        _context.ApplicationUserTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTokenAsync(ApplicationUserToken token)
    {
        _context.ApplicationUserTokens.Remove(token);
        await _context.SaveChangesAsync();
    }

    public async Task<ApplicationUserToken?> GetTokenAsync(string userId, string tokenType, string tokenValue)
    {
        return await _context.ApplicationUserTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
             t.UserId.ToString() == userId && t.TokenType == tokenType && t.TokenValue == tokenValue);
    }

    public async Task<List<ApplicationUserToken>?> GetUserTokensAsync(Guid userId, string tokenType)
    {
        return await _context.ApplicationUserTokens
            .Where(t => t.UserId == userId && t.TokenType == tokenType)
            .ToListAsync();
    }

    public async Task UpdateTokenAsync(ApplicationUserToken token)
    {
        _context.ApplicationUserTokens.Update(token);
        await _context.SaveChangesAsync();
    }
}

