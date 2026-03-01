using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Threading.Channels;
using SearchOrchestrator.Infrastructure.Clients;
using SearchOrchestrator.Infrastructure.Repositories;
using SearchOrchestrator.Infrastructure.Workers;

namespace SearchOrchestrator.Infrastructure;

public static class DependencyInjections
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IIndexTaskRepository, InMemoryTaskRepository>();

        services.AddSingleton(Channel.CreateUnbounded<Guid>());

        services.AddSingleton<ISearchEngineClient, MockSearchEngineClient>();

        services.AddHostedService<IndexTaskBackgroundService>();

        return services;
    }
}