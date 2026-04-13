using System.Reflection;
using ChessTournaments.Modules.Matches.API.Common;
using ChessTournaments.Modules.Matches.Domain.Matches;
using ChessTournaments.Modules.Matches.Infrastructure.Persistence;
using ChessTournaments.Modules.Matches.Infrastructure.Repositories;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Extensions;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.Matches.API;

public static class MatchesModule
{
    public static IServiceCollection AddMatchesModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Add DbContext
        services.AddDbContext<MatchesDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(MatchesDbContext).Assembly.FullName)
            )
        );

        // Add MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(Application.Features.GetMatchById.GetMatchByIdQuery).Assembly
            )
        );

        // Add repositories
        services.AddScoped<IMatchRepository, MatchRepository>();

        // Outbox/Inbox patterns for reliable messaging
        services.AddOutboxInboxPatterns<MatchesDbContext>();

        // Register this module's IOutboxMessagePublisher as non-keyed for injection into handlers
        // This ensures handlers in this module get the correct DbContext-bound publisher
        services.AddScoped<IOutboxMessagePublisher>(sp =>
            sp.GetRequiredKeyedService<IOutboxMessagePublisher>(nameof(MatchesDbContext))
        );

        return services;
    }

    public static IEndpointRouteBuilder MapMatchesEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                typeof(IEndpoint).IsAssignableFrom(t)
                && t is { IsInterface: false, IsAbstract: false }
            );

        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndpoint)Activator.CreateInstance(type)!;
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
