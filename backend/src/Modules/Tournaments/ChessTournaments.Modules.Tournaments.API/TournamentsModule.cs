using ChessTournaments.Modules.Tournaments.Application.Features.GetAllTournaments;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Modules.Tournaments.Infrastructure.Persistence;
using ChessTournaments.Modules.Tournaments.Infrastructure.Repositories;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Extensions;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.Tournaments;

public static class TournamentsModule
{
    public static IServiceCollection AddTournamentsModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Database
        services.AddDbContext<TournamentsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b =>
                {
                    b.MigrationsAssembly(typeof(TournamentsDbContext).Assembly.FullName);
                    b.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        TournamentsDbContext.SchemaName
                    );
                }
            )
        );

        // Repositories
        services.AddScoped<ITournamentRepository, TournamentRepository>();

        // MediatR - Register handlers from Application assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(GetAllTournamentsQuery).Assembly)
        );

        // FluentValidation - Register validators from Application assembly
        services.AddValidatorsFromAssembly(typeof(GetAllTournamentsQuery).Assembly);

        // Outbox/Inbox patterns for reliable messaging
        services.AddOutboxInboxPatterns<TournamentsDbContext>();

        // Register this module's IOutboxMessagePublisher as non-keyed for injection into handlers
        // This ensures handlers in this module get the correct DbContext-bound publisher
        services.AddScoped<IOutboxMessagePublisher>(sp =>
            sp.GetRequiredKeyedService<IOutboxMessagePublisher>(nameof(TournamentsDbContext))
        );

        return services;
    }
}
