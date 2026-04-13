using ChessTournaments.Modules.Players.Application.Features.CreatePlayer;
using ChessTournaments.Modules.Players.Domain.Achievements;
using ChessTournaments.Modules.Players.Domain.Players;
using ChessTournaments.Modules.Players.Infrastructure.Persistence;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Extensions;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.Players.Infrastructure;

public static class PlayersModuleExtensions
{
    public static IServiceCollection AddPlayersModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Register DbContext
        services.AddDbContext<PlayersDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsHistoryTable("__EFMigrationsHistory", "players")
            )
        );

        // Register repositories
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IAchievementRepository, AchievementRepository>();

        // Register MediatR handlers from Application assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreatePlayerCommand).Assembly)
        );

        // Outbox/Inbox patterns for reliable messaging
        services.AddOutboxInboxPatterns<PlayersDbContext>();

        // Register this module's IOutboxMessagePublisher as non-keyed for injection into handlers
        // This ensures handlers in this module get the correct DbContext-bound publisher
        services.AddScoped<IOutboxMessagePublisher>(sp =>
            sp.GetRequiredKeyedService<IOutboxMessagePublisher>(nameof(PlayersDbContext))
        );

        return services;
    }
}
