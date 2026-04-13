using Carter;
using ChessTournaments.Modules.Players.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.Players.API;

public static class PlayersModule
{
    public static IServiceCollection AddPlayersApiModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Add infrastructure layer (DbContext, repositories, MediatR)
        services.AddPlayersModule(configuration);

        // Carter will auto-discover PlayersEndpointGroup
        return services;
    }
}
