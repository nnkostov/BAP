using BAP.Core.Interfaces;
using BAP.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BAP.Core;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register BAP.Core services into the DI container.
    /// </summary>
    public static IServiceCollection AddBapCore(this IServiceCollection services)
    {
        services.AddSingleton<IProjectRepository, JsonProjectRepository>();
        return services;
    }
}
