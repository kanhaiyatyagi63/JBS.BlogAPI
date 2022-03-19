using AutoMapper;
using JBS.DataLayer;
using JBS.DataLayer.Abstracts;
using JBS.Model.RequestModels.Auth;
using JBS.Service.Abstracts;
using JBS.Utility.Constants;
using JBS.Utility.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace JBS.Service;
public class ApplicationUserService : IApplicationUserService
{

    private readonly IUserContextService _userContextService;
    private readonly INotificationService _notificationService;

    private readonly ILogger<ApplicationUserService> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly string _tokenDeliminator = "||";

    public ApplicationUserService(
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IOptions<IdentityOptions> identityOptions,
        ILogger<ApplicationUserService> logger,
        IUserContextService userContextService,
        INotificationService notificationService,
    IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
        _identityOptions = identityOptions;
        _logger = logger;
        _userContextService = userContextService;
        _notificationService = notificationService;
    }

    #region Properties
    public IQueryable<AppUser> Users { get { return _userManager.Users; } }
    #endregion

    public async Task<IdentityResult> AddToRoleAsync(AppUser user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> CreateAsync(AppUser user, string password)
    {
        if (string.IsNullOrEmpty(password))
            return await _userManager.CreateAsync(user);

        return await _userManager.CreateAsync(user, password);
    }
    public async Task<IdentityResult> UpdateAsync(AppUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(AppUser user)
    {
        return await _userManager.DeleteAsync(user);
    }

    public async Task<AppUser> FindByIdAsync(string userId, UserType userType)
    {
        IQueryable<AppUser> query = _userManager.Users.Where(x => x.Id == userId);

        return await query.SingleOrDefaultAsync();
    }

    public async Task<AppUser> FindByNameAsync(string userName)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    public async Task<bool> IsInRoleAsync(AppUser user, string role)
    {
        return await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<AppUser> FindByUserNameOrEmail(string userNameOrEmail)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(userNameOrEmail);
            if (user != null)
            {
                return user;
            }

            return await _userManager.Users.FirstOrDefaultAsync(x => x.IsDeleted == false && x.Email == userNameOrEmail);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<(bool IsValid, string ErrorMessage, AppUser User, IEnumerable<string> Roles)> ValidateUser(string userName, string password, bool isForChangePassword = false)
    {
        var user = await FindByUserNameOrEmail(userName);

        if (user == null || user.IsDeleted)
        {
            return (false, "The username and password do not match, please try again or reset the password.", null, null);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return (false, "Your account is locked out due to too many failed login attempts. A link has already been sent to your configured email to unlock your account.", null, null);
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            if (isForChangePassword)
            {
                return (false, "Current Password is not correct.", null, null);
            }
            else
            {
                await _userManager.AccessFailedAsync(user);

                //Generating unlock user link if user locks out due to exceeding failed attempts and locking user till its password reset
                if (await _userManager.IsLockedOutAsync(user))
                {
                    await GenerateSendUnlockAccountLink(userName);
                    return (false, "Your account is locked due to too many failed login attempts. A link has been sent to your configured email to unlock your account.", null, null);
                }
                return (false, "The username and password do not match, please try again or reset the password.", null, null);
            }
        }

        if (!user.EmailConfirmed)
        {
            return (false, "Your email is not verified yet. Please verify your email and create password.", null, null);
        }

        if (!user.IsActive)
        {
            return (false, "Your account is inactive. Please contact to administrator.", null, null);
        }

        IList<string> roleNames = null;
        if (!isForChangePassword)
        {
            //Get User Role Name
            roleNames = await _userManager.GetRolesAsync(user);
        }

        return (true, null, user, roleNames);
    }

    public async Task<(bool IsValid, string ErrorMessage, AppUser User, IEnumerable<string> Roles)> ValidateUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || user.IsDeleted)
        {
            return (false, "The username and password do not match, please try again or reset the password.", null, null);
        }

        if (!user.IsActive)
        {
            return (false, "Your account is inactive. Please contact to administrator.", null, null);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return (false, "Your account is locked out due to too many failed login attempts. A link has already been sent to your configured email to unlock your account.", null, null);
        }

        //Get User Role Name
        var roleNames = await _userManager.GetRolesAsync(user);

        return (true, null, user, roleNames);
    }

    public async Task<(bool Succeeded, string ErrorMessage)> RecoverAccount(string userNameOrEmail)
    {
        var user = await FindByUserNameOrEmail(userNameOrEmail);
        if (user == null)
        {
            _logger.LogError("User not found. Email/User Id: " + userNameOrEmail);
            return (false, "User not found");
        }

        if (!user.EmailConfirmed)
        {
            return (false, "Account verification pending. Please check the email and verify the account.");
        }

        var secret = Guid.NewGuid().ToString("N");
        var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        //Validate old claims and remove old password claim if exists 
        var userClaims = await _userManager.GetClaimsAsync(user);
        var passwordResetTokenClaim = userClaims.SingleOrDefault(x => x.Type.Equals(CustomClaimTypesConstants.TemporaryPasswordToken));
        if (passwordResetTokenClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, passwordResetTokenClaim);
        }

        //Generate New Claims
        var token = string.Join(_tokenDeliminator, DateTime.UtcNow.AddHours(12).Ticks, secret, passwordResetToken);
        passwordResetTokenClaim = new Claim(CustomClaimTypesConstants.TemporaryPasswordToken, token);
        await _userManager.AddClaimAsync(user, passwordResetTokenClaim);

        await _notificationService.SendForgotPasswordNotificationsAsync(user, secret);

        return (true, null);
    }

    public async Task<(bool Succeeded, IEnumerable<string> ErrorMessage)> ResetAccount(AccountResetModel model)
    {
        _logger.LogDebug($"Reset Account: Check user exist for user id {model.Key}");
        var user = await _userManager.FindByIdAsync(model.Key);
        if (user == null)
        {
            return (false, new List<string>() { "Invalid key or secret" });
        }

        _logger.LogDebug($"Reset Account: Get claims for user with id {model.Key}");
        var userClaims = await _userManager.GetClaimsAsync(user);
        var resetPasswordTokenClaim = userClaims.FirstOrDefault(x => x.Type.Equals(CustomClaimTypesConstants.TemporaryPasswordToken));
        if (resetPasswordTokenClaim == null)
        {
            return (false, new List<string>() { "Either key or secret is invalid or link is already used, please regenerate reset link once again" });
        }

        _logger.LogDebug($"Reset Account: Validate Secret {model.Key}");
        var claimValues = resetPasswordTokenClaim.Value.Split(_tokenDeliminator);
        var expiryTicks = Convert.ToInt64(claimValues[0]);
        var secret = claimValues[1];
        var token = claimValues[2];

        _logger.LogDebug($"Reset Account: Validate Secret for user {model.Key}");
        if (!secret.Equals(model.Secret, StringComparison.CurrentCultureIgnoreCase))
        {
            return (false, new List<string>() { "Invalid key or secret" });
        }


        _logger.LogDebug($"Reset Account: Validate Expiry duration for user {model.Key}");
        if (expiryTicks < DateTime.UtcNow.Ticks)
        {
            await _userManager.RemoveClaimAsync(user, resetPasswordTokenClaim);
            return (false, new List<string>() { "Expired temporary password, please regenerate reset link once again" });
        }

        _logger.LogDebug($"Reset Account: Validate Recent Passwords for user {model.Key}");
        // todo check passowrd history
        //if (await CheckPasswordHistory(user, model.NewPassword))
        //{
        //    return (false, new List<string>() { "You have recently used this password. Please try another password." });
        //}

        _logger.LogDebug($"Reset Account: Reset password expiry for user {model.Key}");
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(x => x.Description));
        }

        await _userManager.RemoveClaimAsync(user, resetPasswordTokenClaim);

        //Unlocking User on password change
        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogDebug($"Reset Account: Unlocking user {model.Key}");
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now);
        }

        //Maintaing Password History
        await InsertUserPasswordAsync(user.Id, user.PasswordHash);

        return (true, null);
    }

    public async Task GenerateSendUnlockAccountLink(string userNameOrEmail)
    {
        var user = await FindByUserNameOrEmail(userNameOrEmail);
        if (user == null)
        {
            _logger.LogError($"GenerateSendUnlockAccountLink: User not found. Email/User Id: {userNameOrEmail}");
            throw new ArgumentException("GenerateSendUnlockAccountLink failed: User {userNameOrEmail} not found");
        }

        _logger.LogError($"GenerateSendUnlockAccountLink: Locking user {userNameOrEmail} till it resets its password ");
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);


        //Validate old claims and remove old password claim if exists 
        _logger.LogError($"GenerateSendUnlockAccountLink: Remove old password claim if exists for user {userNameOrEmail}");
        var userClaims = await _userManager.GetClaimsAsync(user);
        var passwordResetTokenClaim = userClaims.SingleOrDefault(x => x.Type.Equals(CustomClaimTypesConstants.TemporaryPasswordToken));
        if (passwordResetTokenClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, passwordResetTokenClaim);
        }

        //Generate New Claims
        _logger.LogError($"GenerateSendUnlockAccountLink: Generating New link for user {userNameOrEmail}");
        var secret = Guid.NewGuid().ToString("N");
        var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var token = string.Join(_tokenDeliminator, DateTime.UtcNow.AddHours(12).Ticks, secret, passwordResetToken);
        passwordResetTokenClaim = new Claim(CustomClaimTypesConstants.TemporaryPasswordToken, token);
        await _userManager.AddClaimAsync(user, passwordResetTokenClaim);

        await _notificationService.SendAccountUnlockNotificationsAsync(user, secret);
    }

    public async Task<(bool Succeeded, IEnumerable<string> ErrorMessage)> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
    {
        _logger.LogDebug($"ChangePasswordAsync: Validate Recent Passwords for user {user.UserName}");

        _logger.LogDebug($"ChangePasswordAsync: Validating current Password for user {user.UserName}");
        var currentPasswordValidationResult = await ValidateUser(user.UserName, currentPassword, true);
        if (!currentPasswordValidationResult.IsValid)
        {
            return (false, new List<string> { currentPasswordValidationResult.ErrorMessage });
        }

        _logger.LogDebug($"ChangePasswordAsync: Validating current Password for user {user.UserName}");
        // todo: Check passsword history
        //if (await CheckPasswordHistory(user, newPassword))
        //{
        //    return (false, new List<string>() { "You have recently set this password. Please try another password." });
        //}

        _logger.LogDebug($"ChangePasswordAsync: Changing Password for user {user.UserName}");
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
        {
            _logger.LogDebug($"ChangePasswordAsync: Changing Password Succeeded for user {user.UserName}");
            await InsertUserPasswordAsync(user.Id, user.PasswordHash);
        }
        else
        {
            _logger.LogDebug($"ChangePasswordAsync: Changing Password failed for user {user.UserName}");
        }

        return (result.Succeeded, result.Errors.Select(x => x.Description));
    }

    public async Task ResetPasswordAsync(AppUser user, string newPassword)
    {
        var result = await _userManager.RemovePasswordAsync(user);
        if (result.Succeeded)
        {
            //_logger.LogInformation($"Password for {user.Id} removed");

            var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);

            if (addPasswordResult.Succeeded)
            {
                //_logger.LogInformation($"Password for {user.Id} changed");
            }
        }
    }

    public async Task<(bool Succeeded, IEnumerable<string> ErrorMessage)> ActivateAccount(AccountActivateModel model)
    {
        _logger.LogDebug($"Activate Account: Check user exist for user id {model.Key}");
        var user = await _userManager.FindByIdAsync(model.Key);
        if (user == null)
        {
            return (false, new List<string>() { "Invalid key or secret" });
        }

        _logger.LogDebug($"Activate Account: Get claims for user with id {model.Key}");
        var userClaims = await _userManager.GetClaimsAsync(user);
        var emailConfirmationTokenClaim = userClaims.FirstOrDefault(x => x.Type.Equals(CustomClaimTypesConstants.EmailConfirmationToken));
        if (emailConfirmationTokenClaim == null)
        {
            return (false, new List<string>() { "Invalid key or secret" });
        }

        _logger.LogDebug($"Activate Account: Validate Secret {model.Key}");
        var claimValues = emailConfirmationTokenClaim.Value.Split(_tokenDeliminator);
        var secret = claimValues[0];
        var token = claimValues[1];
        if (!model.Secret.Equals(secret, StringComparison.CurrentCultureIgnoreCase))
        {
            return (false, new List<string>() { "Invalid key or secret" });
        }

        IdentityResult result;
        if (!user.EmailConfirmed)
        {
            _logger.LogDebug($"Activate Account: Confirm Email is  user with id {model.Key}");
            result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(x => x.Description));
            }

            _logger.LogDebug($"Activate Account: Email confirmed hence remove claim");
            await _userManager.RemoveClaimAsync(user, emailConfirmationTokenClaim);
        }

        _logger.LogDebug($"Activate Account: Add Password for user {model.Key}");
        result = await _userManager.AddPasswordAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(x => x.Description));
        }
        await InsertUserPasswordAsync(model.Key, user.PasswordHash);

        return (true, null);
    }

    public async Task<IList<string>> GetUserRoles(AppUser user)
    {

        return await _userManager.GetRolesAsync(user);

    }

    private async Task<string> GetUniqueUserName(string name)
    {
        //_logger.LogDebug("GetUnique User Name Started");

        long counter = 0;
        string generatedUserName = GenerateUserName(name);
        var originalGeneratedUserName = generatedUserName.Clone();

        var userNames = await _userManager.Users.Where(x => x.UserName.StartsWith(generatedUserName)).ToListAsync();
        while (!userNames.Any(x => x.Equals(generatedUserName)))
        {
            generatedUserName = string.Concat(originalGeneratedUserName, ++counter);
        }
        //_logger.LogDebug("GetUnique User Name Completed");
        return generatedUserName;
    }
    private static string GenerateUserName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        string generatedUserName;

        //Break string into words
        var words = name.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 1)
        {
            //All except last
            var exceptLast = words.Take(words.Length - 1);
            generatedUserName = string.Concat(string.Join("", exceptLast.Select(x => x[0])), words.Last());
        }
        else
        {
            generatedUserName = name;
        }

        return generatedUserName;
    }
    public async Task<AppUser> GetUser(string email)
    {
        try
        {
            var result = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower() &&
             !string.IsNullOrEmpty(email) && x.EmailConfirmed == true);
            return result;


        }
        catch (Exception)
        {

        }
        return null;
    }

    public async Task<string> GetProfileImageUrl(string profileImage)
    {
        if (string.IsNullOrEmpty(profileImage))
            return null;
        return String.Empty;

    }

    public async Task<string> ResendAccountVerificationEmail(string[] emails)
    {
        var users = _userManager.Users.Where(x => emails.Contains(x.Email) && !x.EmailConfirmed).ToList();
        if (users.Count() == 0)
        {
            return "Invalid emails";
        }
        foreach (var user in users)
        {
            _logger.LogDebug($"AddUserAsync: Create Activation Claim and Url");
            var claims = await _userManager.GetClaimsAsync(user);
            var accountConfirmationEmailClaim = claims.FirstOrDefault(x => x.Type.Equals(CustomClaimTypesConstants.EmailConfirmationToken));
            if (accountConfirmationEmailClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, accountConfirmationEmailClaim);
            }
            var secret = Guid.NewGuid().ToString("N");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var claim = new Claim(CustomClaimTypesConstants.EmailConfirmationToken, string.Join(_tokenDeliminator, secret, token));
            await _userManager.AddClaimAsync(user, claim);
            _logger.LogDebug($"AddUserAsync: Create Activation Claim");

            _logger.LogDebug($"AddUserAsync: Send Welcome email");
            //await _notificationService.SendAccountCreationEmail(user, secret);
        }
        return "Account creation email sent successfully.";
    }



    public async Task DeleteAsync(string[] userIds, UserType userType)
    {
        _logger.LogInformation($"Delete {userType.ToString()} Started");
        foreach (var userId in userIds)
        {
            _logger.LogInformation($"Deleting {userType.ToString()} for Id: {userId}");

            var user = await FindByIdAsync(userId, userType);
            if (user == null || userId == _userContextService.GetUserId())
                throw new Exception($"{userType.ToString()} deletion failed. Error: Invalid {userType.ToString()}");

            if (user.IsDeleted)
                throw new Exception($"{userType.ToString()} deletion failed. Error: {userType.ToString()} already deleted");

            user.IsDeleted = true;

            await UpdateAsync(user);
            _logger.LogInformation($"Delete {userType.ToString()} Successfull for Id  {user.Id}");
        }

        await _unitOfWork.CommitAsync();
    }

    public async Task UnDeleteAsync(string[] userIds, UserType userType)
    {
        _logger.LogInformation($"UnDelete {userType.ToString()} Started");
        foreach (var userId in userIds)
        {
            _logger.LogInformation($"UnDeleting {userType.ToString()} for Id: {userId}");

            var user = await FindByIdAsync(userId, userType);
            if (user == null || userId == _userContextService.GetUserId())
                throw new Exception($"{userType.ToString()} undeletion failed. Error: Invalid {userType.ToString()}");

            if (!user.IsDeleted)
                throw new Exception($"{userType.ToString()} undeletion failed. Error: {userType.ToString()} is not  deleted");

            user.IsDeleted = false;

            await UpdateAsync(user);
            _logger.LogInformation($"UnDelete {userType.ToString()} Successfull for Id  {user.Id}");
        }

        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateName()
    {
        foreach (var user in _userManager.Users)
        {

            user.Name = Regex.Replace(user.Name, @"\s+", " ");
        }

        await _unitOfWork.CommitAsync();
    }

    // todo: Need to create password table
    public async Task InsertUserPasswordAsync(string userId, string passwordHash)
    {
        //_logger.LogDebug($"InsertUserPasswordAsync: Adding Password History for user {userId}");
        //await _unitOfWork.UserPasswordRepository.InsertAsync(new UserPassword()
        //{
        //    UserId = userId,
        //    PasswordHash = passwordHash
        //});

        //_logger.LogDebug($"InsertUserPasswordAsync: saving changes for user {userId}");
        //await _unitOfWork.CommitAsync();
        //_logger.LogDebug($"InsertUserPasswordAsync: Password History added successfully for userId: {userId}");
    }

    //public async Task<bool> CheckPasswordHistory(AppUser user, string newPassword)
    //{
    //    var successResults = new List<PasswordVerificationResult>() { PasswordVerificationResult.Success, PasswordVerificationResult.SuccessRehashNeeded };

    //    _logger.LogDebug($"GetUserPasswordsByUserIdAsync: Checking Password History for user {user.Id}");
    //    return (await _unitOfWork.UserPasswordRepository.Filter(x => x.UserId == user.Id && !x.IsDeleted)
    //            .OrderByDescending(x => x.CreatedDate)
    //            .Take(12)
    //            .ToListAsync())
    //            .Any(x => successResults.Contains(_userManager.PasswordHasher.VerifyHashedPassword(user, x.PasswordHash, newPassword)));
    //}



    public async Task<bool> IsUserIdExist(string id, UserType userType)
    {
        var user = await FindByIdAsync(id, userType);
        return user != null && !user.IsDeleted && user.IsActive;
    }


    public async Task<(bool IsSuccess, string Message)> AddUserAsync(AppUser user, AppRole role, IFormFile profileImgFile, string[] tags)
    {
        _logger.LogDebug($"AddUserAsync: Upload Profile pic Started");
        string profileImg = null;

        if (profileImgFile != null)
        {
            profileImg = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMdd}{Path.GetExtension(profileImgFile.FileName)}";
        }

        user.ProfileImage = profileImg;
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("AddUserAsync: User Creation failed");
            _logger.LogError(string.Join(",", result.Errors.Select(x => x.Description)));
            if (!string.IsNullOrEmpty(profileImg))
            {
            }

            throw new Exception($"User Creation failed, {result.Errors.Select(x => x.Description)}");
        }
        _logger.LogDebug($"AddUserAsync: Create User Succeeded");

        _logger.LogDebug($"AddUserAsync: Create Role Started");

        var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
        if (!roleResult.Succeeded)
        {
            _logger.LogError($"AddUserAsync: Createing roles for user {user.Id} failed.");
            _logger.LogError(string.Join(",", roleResult.Errors.Select(x => x.Description)));

            await _userManager.DeleteAsync(user);
            if (!string.IsNullOrEmpty(profileImg))
            {
            }
            throw new Exception($"Createing roles for user {user.Id} failed. Error {roleResult.Errors.Select(x => x.Description)}");
        }
        _logger.LogDebug($"AddUserAsync: Create Roles Succeeded");

        try
        {
            await _unitOfWork.CommitAsync();
        }
        catch (Exception)
        {

        }

        _logger.LogDebug($"AddUserAsync: Create Activation Claim and Url");
        var secret = Guid.NewGuid().ToString("N");
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var claim = new Claim(CustomClaimTypesConstants.EmailConfirmationToken, string.Join(_tokenDeliminator, secret, token));
        await _userManager.AddClaimAsync(user, claim);
        _logger.LogDebug($"AddUserAsync: Create Activation Claim");

        _logger.LogDebug($"AddUserAsync: Send Welcome email");
        await _notificationService.SendAccountCreationEmail(user, secret);
        return (true, "Staff created successfully.");
    }

    public async Task UpdateUserAsync(AppUser user, UserType userType, IFormFile profileImgFile, string role, string[] tags)
    {

        _logger.LogDebug("UpdateUserAsync: Validate Role Starts");
        var userRole = await _roleManager.FindByIdAsync(role);
        if (userRole == null || userRole.IsDeleted)
        {
            throw new ArgumentException("Role does not exists");
        }

        _logger.LogDebug($"UpdateUserAsync: Validation Completed");


        if (profileImgFile != null)
        {
            _logger.LogDebug($"UpdateUserAsync: Upload Profile pic Started");
            user.ProfileImage = !string.IsNullOrEmpty(user.ProfileImage) ? user.ProfileImage :
                $"{Guid.NewGuid()}_{DateTime.UtcNow.ToString("yyyyMMdd")}{Path.GetExtension(profileImgFile.FileName)}";
            // todo: operation for user image
            _logger.LogDebug($"UpdateUserAsync: Upload Profile pic Completed");
        }

        _logger.LogDebug($"UpdateUserAsync: Update User");

        var result = await UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("UpdateUserAsync: User updation failed");
            _logger.LogError(string.Join(",", result.Errors.Select(x => x.Description)));

            throw new Exception($"User Updation failed, {result.Errors.Select(x => x.Description)}");
        }
        _logger.LogDebug($"UpdateUserAsync: Update User Succeeded");

        _logger.LogDebug($"UpdateUserAsync: Update Role Started");

        var roles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, roles);
        result = await _userManager.AddToRoleAsync(user, userRole.Name);
        if (!result.Succeeded)
        {
            _logger.LogError($"UpdateUserAsync: Updating roles for user {user.Id} failed.");
            _logger.LogError(string.Join(",", result.Errors.Select(x => x.Description)));
            throw new Exception($"Error : {string.Join(",", result.Errors.Select(x => x.Description))}");
        }

        _logger.LogDebug($"AddUserAsync: Create Roles Succeeded");

        _logger.LogDebug($"UpdateUserAsync: Update tags Started");

        await _unitOfWork.CommitAsync();

        _logger.LogDebug($"UpdateUserAsync: Update tags Completed");
    }
}
