using JBS.Service;
using JBS.Service.Abstracts;
using Microsoft.Extensions.DependencyInjection;

namespace JBS.DataLayer.Abstracts;
public static class ServiceExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<ISeedService, SeedService>();
        services.AddScoped<IApplicationRoleService, ApplicationRoleService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IApplicationUserService, ApplicationUserService>();
        return services;
    }
}
