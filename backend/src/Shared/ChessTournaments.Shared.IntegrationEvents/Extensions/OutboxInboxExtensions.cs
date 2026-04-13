using ChessTournaments.Shared.IntegrationEvents.Inbox;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Shared.IntegrationEvents.Extensions;

/// <summary>
/// Extension methods for registering Outbox and Inbox pattern services
/// </summary>
public static class OutboxInboxExtensions
{
    /// <summary>
    /// Adds Outbox pattern services for a specific DbContext
    /// Uses keyed services to avoid conflicts when multiple modules register the same interface
    /// </summary>
    public static IServiceCollection AddOutboxPattern<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        // Use keyed service with the DbContext type name as the key
        var key = typeof(TDbContext).Name;

        services.AddKeyedScoped<IOutboxMessagePublisher>(
            key,
            (sp, _) =>
            {
                var dbContext = sp.GetRequiredService<TDbContext>();
                var logger =
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<OutboxMessagePublisher>>();
                return new OutboxMessagePublisher(dbContext, logger);
            }
        );

        services.AddHostedService<OutboxMessageProcessor<TDbContext>>();

        return services;
    }

    /// <summary>
    /// Adds Inbox pattern services for a specific DbContext
    /// Registers DbContext as keyed service for use by ModuleAwareInboxPublisher
    /// </summary>
    public static IServiceCollection AddInboxPattern<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        // Use keyed service with the DbContext type name as the key
        var key = typeof(TDbContext).Name;

        // Register DbContext as keyed service so ModuleAwareInboxPublisher can resolve it
        services.AddKeyedScoped<DbContext>(key, (sp, _) => sp.GetRequiredService<TDbContext>());

        services.AddKeyedScoped<IInboxMessageHandler>(
            key,
            (sp, _) =>
            {
                var dbContext = sp.GetRequiredService<TDbContext>();
                var publisher = sp.GetRequiredService<MediatR.IPublisher>();
                var logger =
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<InboxMessageHandler>>();
                return new InboxMessageHandler(dbContext, publisher, logger);
            }
        );

        return services;
    }

    /// <summary>
    /// Adds both Outbox and Inbox patterns for a specific DbContext
    /// </summary>
    public static IServiceCollection AddOutboxInboxPatterns<TDbContext>(
        this IServiceCollection services
    )
        where TDbContext : DbContext
    {
        services.AddOutboxPattern<TDbContext>();
        services.AddInboxPattern<TDbContext>();

        return services;
    }

    /// <summary>
    /// Registers the ModuleAwareInboxPublisher as the application-wide INotificationPublisher
    /// This should be called ONCE at the application level after all modules are registered
    /// </summary>
    public static IServiceCollection AddModuleAwareInboxPublisher(this IServiceCollection services)
    {
        services.AddScoped<INotificationPublisher, ModuleAwareInboxPublisher>();
        return services;
    }
}
