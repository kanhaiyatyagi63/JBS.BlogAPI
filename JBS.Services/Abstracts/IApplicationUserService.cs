using JBS.DataLayer;
using JBS.Model.RequestModels.Auth;
using JBS.Utility.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace JBS.Service.Abstracts;
public interface IApplicationUserService
{
    Task<AppUser> FindByIdAsync(string userId, UserType userType);
    IQueryable<AppUser> Users { get; }
    Task<AppUser> FindByNameAsync(string userName);
    Task<AppUser> FindByUserNameOrEmail(string userNameOrEmail);
    Task<IdentityResult> CreateAsync(AppUser user, string password);
    Task<IdentityResult> UpdateAsync(AppUser user);
    Task<(bool Succeeded, IEnumerable<string> ErrorMessage)> ActivateAccount(AccountActivateModel model);
    Task<(bool Succeeded, string ErrorMessage)> RecoverAccount(string userNameOrEmail);
    Task<(bool Succeeded, IEnumerable<string> ErrorMessage)> ResetAccount(AccountResetModel model);
    Task<IList<string>> GetUserRoles(AppUser user);
    Task<bool> IsInRoleAsync(AppUser user, string role);
    Task<IdentityResult> AddToRoleAsync(AppUser user, string role);
    Task<IdentityResult> DeleteAsync(AppUser user);
    Task<(bool IsValid, string ErrorMessage, AppUser User, IEnumerable<string> Roles)> ValidateUser(string userName, string password, bool isForChangePassword = false);
    Task<(bool IsValid, string ErrorMessage, AppUser User, IEnumerable<string> Roles)> ValidateUser(string userId);
    Task<(bool Succeeded, IEnumerable<string> ErrorMessage)> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword);
    Task ResetPasswordAsync(AppUser user, string newPassword);
    Task<AppUser> GetUser(string email);
    Task<string> GetProfileImageUrl(string profileImage);
    Task<string> ResendAccountVerificationEmail(string[] emails);

    Task UpdateName();

    Task GenerateSendUnlockAccountLink(string userNameOrEmail);
    Task InsertUserPasswordAsync(string userId, string passwordHash);
    //Task<bool> CheckPasswordHistory(AppUser user, string newPassword);
    Task<bool> IsUserIdExist(string id, UserType userType);
    Task<(bool IsSuccess, string Message)> AddUserAsync(AppUser user, AppRole role, IFormFile profileImgFile, string[] tags);
    Task UpdateUserAsync(AppUser user, UserType userType, IFormFile profileImgFile, string role, string[] tags);
    Task DeleteAsync(string[] userIds, UserType userType);
    Task UnDeleteAsync(string[] userIds, UserType userType);
}
