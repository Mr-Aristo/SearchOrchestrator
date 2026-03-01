using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SearchOrchestrator.Application.Services;

namespace SearchOrchestrator.Application;

public static class DependencyInjections
{
    /// <summary>
    /// Adds application-specific services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which the application services will be added.</param>
    /// <param name="configuration">The configuration settings used to configure the application services.</param>
    /// <returns>The same service collection instance with the application services registered.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<OrchestratorService>();

        return services;
    }
}
