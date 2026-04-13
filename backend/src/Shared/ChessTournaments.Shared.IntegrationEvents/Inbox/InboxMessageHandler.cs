using System.Text.Json;
using ChessTournaments.Shared.Domain.Inbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChessTournaments.Shared.IntegrationEvents.Inbox;

/// <summary>
/// Handles integration events with idempotency using the Inbox pattern
/// </summary>
public class InboxMessageHandler : IInboxMessageHandler
{
    private readonly DbContext _dbContext;
    private readonly IPublisher _publisher;
    private readonly ILogger<InboxMessageHandler> _logger;

    public InboxMessageHandler(
        DbContext dbContext,
        IPublisher publisher,
        ILogger<InboxMessageHandler> logger
    )
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<bool> TryProcessAsync(
        Guid messageId,
        string messageType,
        string content,
        CancellationToken cancellationToken = default
    )
    {
        // Check if message already exists in inbox (idempotency check)
        var existingMessage = await _dbContext
            .Set<InboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (existingMessage != null)
        {
            _logger.LogInformation(
                "Message {MessageId} already processed, skipping (idempotency)",
                messageId
            );
            return true;
        }

        // Create inbox message
        var inboxMessage = new InboxMessage(messageId, messageType, content, DateTime.UtcNow);

        await _dbContext.Set<InboxMessage>().AddAsync(inboxMessage, cancellationToken);

        try
        {
            // Deserialize and process the event
            var eventType = Type.GetType(messageType);
            if (eventType == null)
            {
                _logger.LogWarning(
                    "Could not resolve type {Type} for message {MessageId}",
                    messageType,
                    messageId
                );
                inboxMessage.MarkAsFailed($"Could not resolve type: {messageType}");
                await _dbContext.SaveChangesAsync(cancellationToken);
                return false;
            }

            var integrationEvent =
                JsonSerializer.Deserialize(content, eventType) as IIntegrationEvent;

            if (integrationEvent != null)
            {
                await _publisher.Publish(integrationEvent, cancellationToken);
                inboxMessage.MarkAsProcessed();

                _logger.LogInformation(
                    "Processed inbox message {MessageId} of type {Type}",
                    messageId,
                    messageType
                );
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbox message {MessageId}", messageId);
            inboxMessage.MarkAsFailed(ex.Message);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }
    }
}
