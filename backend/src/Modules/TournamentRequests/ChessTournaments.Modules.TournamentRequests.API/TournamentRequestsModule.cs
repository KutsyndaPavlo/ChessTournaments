using ChessTournaments.Modules.TournamentRequests.Application.Features.GetAllTournamentRequests;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using ChessTournaments.Modules.TournamentRequests.Infrastructure.Persistence;
using ChessTournaments.Modules.TournamentRequests.Infrastructure.Persistence.Repositories;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Extensions;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.TournamentRequests;

public static class TournamentRequestsModule
{
    public static IServiceCollection AddTournamentRequestsModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Database
        services.AddDbContext<TournamentRequestsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b =>
                {
                    b.MigrationsAssembly(typeof(TournamentRequestsDbContext).Assembly.FullName);
                    b.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        TournamentRequestsDbContext.SchemaName
                    );
                }
            )
        );

        // Repositories
        services.AddScoped<ITournamentRequestRepository, TournamentRequestRepository>();

        // MediatR - Register handlers from Application assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(GetAllTournamentRequestsQuery).Assembly)
        );

        // FluentValidation - Register validators from Application assembly
        services.AddValidatorsFromAssembly(typeof(GetAllTournamentRequestsQuery).Assembly);

        // Outbox/Inbox patterns for reliable messaging
        services.AddOutboxInboxPatterns<TournamentRequestsDbContext>();

        // Register this module's IOutboxMessagePublisher as non-keyed for injection into handlers
        // This ensures handlers in this module get the correct DbContext-bound publisher
        services.AddScoped<IOutboxMessagePublisher>(sp =>
            sp.GetRequiredKeyedService<IOutboxMessagePublisher>(nameof(TournamentRequestsDbContext))
        );

        return services;
    }
}
