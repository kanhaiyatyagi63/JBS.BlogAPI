using JBS.DataLayer;
using JBS.Service.Abstracts;

namespace JBS.Service;
internal class NotificationService : INotificationService
{
    public Task SendAccountCreationEmail(AppUser user, string secret)
    {
        return Task.FromResult(0);
    }

    public Task SendAccountUnlockNotificationsAsync(AppUser user, string secret)
    {
        return Task.FromResult(0);
    }

    public Task SendForgotPasswordNotificationsAsync(AppUser user, string secret)
    {
        return Task.FromResult(0);
    }
}
