using System.Text.Json;
using ChessTournaments.Shared.Domain.Inbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChessTournaments.Shared.IntegrationEvents.Inbox;

/// <summary>
/// Module-aware MediatR notification publisher that routes integration events to the correct module's DbContext for Inbox storage
/// Determines the target module by examining the handler's namespace
/// </summary>
public class ModuleAwareInboxPublisher : INotificationPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ModuleAwareInboxPublisher> _logger;

    public ModuleAwareInboxPublisher(
        IServiceProvider serviceProvider,
        ILogger<ModuleAwareInboxPublisher> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken
    )
    {
        // Only apply Inbox pattern to integration events
        if (notification is not IIntegrationEvent integrationEvent)
        {
            // For non-integration events, use default behavior
            foreach (var handler in handlerExecutors)
            {
                await handler
                    .HandlerCallback(notification, cancellationToken)
                    .ConfigureAwait(false);
            }
            return;
        }

        // Process each handler separately with its own inbox
        foreach (var handlerExecutor in handlerExecutors)
        {
            await ProcessHandlerWithInbox(handlerExecutor, integrationEvent, cancellationToken);
        }
    }

    private async Task ProcessHandlerWithInbox(
        NotificationHandlerExecutor handlerExecutor,
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken
    )
    {
        // Determine which module this handler belongs to based on namespace
        var handlerType = handlerExecutor.HandlerInstance.GetType();
        var dbContextKey = DetermineDbContextKey(handlerType);

        if (dbContextKey == null)
        {
            _logger.LogWarning(
                "Could not determine DbContext for handler {HandlerType}. Executing without Inbox pattern.",
                handlerType.Name
            );
            await handlerExecutor.HandlerCallback(integrationEvent, cancellationToken);
            return;
        }

        _logger.LogInformation(
            "Processing integration event {EventType} with handler {HandlerType} using DbContext key {DbContextKey}",
            integrationEvent.GetType().Name,
            handlerType.Name,
            dbContextKey
        );

        // Get the module-specific DbContext
        var dbContext = _serviceProvider.GetRequiredKeyedService<DbContext>(dbContextKey);

        var messageId = integrationEvent.EventId;
        var messageType = integrationEvent.GetType().AssemblyQualifiedName!;
        var content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType());

        // Check if message already exists in this module's inbox (idempotency check)
        var existingMessage = await dbContext
            .Set<InboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (existingMessage != null)
        {
            _logger.LogInformation(
                "Integration event {EventType} with ID {MessageId} already processed by {HandlerType}, skipping (idempotency)",
                integrationEvent.GetType().Name,
                messageId,
                handlerType.Name
            );
            return;
        }

        // Create inbox message
        var inboxMessage = new InboxMessage(messageId, messageType, content, DateTime.UtcNow);
        await dbContext.Set<InboxMessage>().AddAsync(inboxMessage, cancellationToken);

        try
        {
            _logger.LogInformation(
                "Processing integration event {EventType} with ID {MessageId} through Inbox pattern in {DbContextKey}",
                integrationEvent.GetType().Name,
                messageId,
                dbContextKey
            );

            // Execute the handler
            await handlerExecutor.HandlerCallback(integrationEvent, cancellationToken);

            // Mark as processed
            inboxMessage.MarkAsProcessed();

            _logger.LogInformation(
                "Successfully processed integration event {EventType} with ID {MessageId} in {DbContextKey}",
                integrationEvent.GetType().Name,
                messageId,
                dbContextKey
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing integration event {EventType} with ID {MessageId} in {DbContextKey}",
                integrationEvent.GetType().Name,
                messageId,
                dbContextKey
            );

            inboxMessage.MarkAsFailed(ex.Message);
            throw; // Re-throw to let caller know processing failed
        }
        finally
        {
            // Save inbox message state
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Determines the DbContext key based on the handler's namespace
    /// Maps handler namespace to the appropriate DbContext key
    /// </summary>
    private string? DetermineDbContextKey(Type handlerType)
    {
        var handlerNamespace = handlerType.Namespace ?? "";

        if (handlerNamespace.Contains(".Modules.Tournaments."))
            return "TournamentsDbContext";

        if (handlerNamespace.Contains(".Modules.Matches."))
            return "MatchesDbContext";

        if (handlerNamespace.Contains(".Modules.Players."))
            return "PlayersDbContext";

        if (handlerNamespace.Contains(".Modules.TournamentRequests."))
            return "TournamentRequestsDbContext";

        return null;
    }
}
