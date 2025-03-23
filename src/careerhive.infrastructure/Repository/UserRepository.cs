using careerhive.domain.Entities;
using careerhive.application.Interfaces.IRepository;
using careerhive.infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace careerhive.infrastructure.Repository;
public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationDbContext _context;

    public UserRepository(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public async Task<IdentityResult> CreateAsync(User user, string password) 
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetUserAsync(Expression<Func<User, bool>> expression)
    {
        return await _userManager.Users.FirstOrDefaultAsync(expression);
    }

    public async Task<List<User>> GetAllUsersAsync(Expression<Func<User, bool>> expression)
    {
        return await _userManager.Users.Where(expression).ToListAsync();
    }

    public async Task<IdentityResult> UpdateAsync(User user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(User user)
    {
        return await _userManager.DeleteAsync(user);
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
    {
        return await _userManager.RemoveFromRoleAsync(user, role);
    }

    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string emailToken)
    {
        return await _userManager.ConfirmEmailAsync(user, emailToken);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(User user, string emailToken, string newPassword)
    {
        return await _userManager.ResetPasswordAsync(user, emailToken, newPassword);
    }

    public async Task<string> GenerateResetPasswordTokenAsync(User user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> Manage2faAsync(User user, bool enable)
    {
        return await _userManager.SetTwoFactorEnabledAsync(user, enable);
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
    {
        return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
    }

    public async Task<IList<string?>> GetRolesAsync()
    {
        return await _context.Roles.Select(r => r.Name).ToListAsync();
    }
}
