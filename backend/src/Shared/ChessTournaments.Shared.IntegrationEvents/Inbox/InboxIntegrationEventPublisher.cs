using System.Text.Json;
using ChessTournaments.Shared.Domain.Inbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChessTournaments.Shared.IntegrationEvents.Inbox;

/// <summary>
/// Custom MediatR notification publisher for integration events that enforces the Inbox pattern
/// Ensures integration events are processed exactly once (idempotency)
/// </summary>
public class InboxIntegrationEventPublisher : INotificationPublisher
{
    private readonly DbContext _dbContext;
    private readonly ILogger<InboxIntegrationEventPublisher> _logger;

    public InboxIntegrationEventPublisher(
        DbContext dbContext,
        ILogger<InboxIntegrationEventPublisher> logger
    )
    {
        _dbContext = dbContext;
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

        var messageId = integrationEvent.EventId;
        var messageType = integrationEvent.GetType().AssemblyQualifiedName!;
        var content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType());

        // Check if message already exists in inbox (idempotency check)
        var existingMessage = await _dbContext
            .Set<InboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (existingMessage != null)
        {
            _logger.LogInformation(
                "Integration event {EventType} with ID {MessageId} already processed, skipping (idempotency)",
                integrationEvent.GetType().Name,
                messageId
            );
            return;
        }

        // Create inbox message
        var inboxMessage = new InboxMessage(messageId, messageType, content, DateTime.UtcNow);
        await _dbContext.Set<InboxMessage>().AddAsync(inboxMessage, cancellationToken);

        try
        {
            _logger.LogInformation(
                "Processing integration event {EventType} with ID {MessageId} through Inbox pattern",
                integrationEvent.GetType().Name,
                messageId
            );

            // Execute all handlers
            foreach (var handler in handlerExecutors)
            {
                await handler
                    .HandlerCallback(notification, cancellationToken)
                    .ConfigureAwait(false);
            }

            // Mark as processed
            inboxMessage.MarkAsProcessed();

            _logger.LogInformation(
                "Successfully processed integration event {EventType} with ID {MessageId}",
                integrationEvent.GetType().Name,
                messageId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing integration event {EventType} with ID {MessageId}",
                integrationEvent.GetType().Name,
                messageId
            );

            inboxMessage.MarkAsFailed(ex.Message);
            throw; // Re-throw to let caller know processing failed
        }
        finally
        {
            // Save inbox message state
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
