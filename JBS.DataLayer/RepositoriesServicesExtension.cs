using JBS.DataLayer.Abstracts;
using Microsoft.Extensions.DependencyInjection;

namespace JBS.DataLayer;
public static class RepositoriesServicesExtension
{
    public static IServiceCollection ConfigureRepositoriesServices(this IServiceCollection services)
    {
        //Add Unit Of Work Dependencies 
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}