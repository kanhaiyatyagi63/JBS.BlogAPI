using JBS.DataLayer;

namespace JBS.Service.Abstracts;
public interface INotificationService
{
    Task SendAccountCreationEmail(AppUser user, string secret);

    Task SendForgotPasswordNotificationsAsync(AppUser user, string secret);

    Task SendAccountUnlockNotificationsAsync(AppUser user, string secret);

}
