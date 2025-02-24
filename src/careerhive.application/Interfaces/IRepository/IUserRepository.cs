using System.Linq.Expressions;
using careerhive.application.DTOs;
using careerhive.application.DTOs.Request;
using careerhive.domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace careerhive.application.Interfaces.IRepository;
public interface IUserRepository
{
    Task<IdentityResult> CreateAsync(User user, string password);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<User?> GetUserByExpressionAsync(Expression<Func<User, bool>> expression);
    Task<List<User>> GetAllUsersAsync(Expression<Func<User, bool>> expression);
    Task<IdentityResult> UpdateAsync(User user);
    Task<IdentityResult> DeleteAsync(User user);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<IdentityResult> RemoveFromRoleAsync(User user, string role);
    Task<IList<string>> GetRolesAsync(User user);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<IdentityResult> ConfirmEmailAsync(User user, string emailToken);
    Task<IdentityResult> ResetPasswordAsync(User user, string emailToken,string newPassword);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<string> GenerateResetPasswordTokenAsync(User user);

    Task<IdentityResult> Manage2faAsync(User user, bool enable);
    Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);
}
