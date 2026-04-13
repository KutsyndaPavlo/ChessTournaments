using System.Text.Json;
using ChessTournaments.Shared.Domain.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChessTournaments.Shared.IntegrationEvents.Outbox;

/// <summary>
/// Background service that processes outbox messages and publishes them as integration events
/// </summary>
public class OutboxMessageProcessor<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxMessageProcessor<TDbContext>> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);

    public OutboxMessageProcessor(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxMessageProcessor<TDbContext>> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Message Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Message Processor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var outboxMessages = await dbContext
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var outboxMessage in outboxMessages)
        {
            try
            {
                var eventType = Type.GetType(outboxMessage.Type);
                if (eventType == null)
                {
                    _logger.LogWarning(
                        "Could not resolve type {Type} for outbox message {MessageId}",
                        outboxMessage.Type,
                        outboxMessage.Id
                    );
                    outboxMessage.MarkAsFailed($"Could not resolve type: {outboxMessage.Type}");
                    continue;
                }

                var integrationEvent =
                    JsonSerializer.Deserialize(outboxMessage.Content, eventType)
                    as IIntegrationEvent;

                if (integrationEvent != null)
                {
                    await publisher.Publish(integrationEvent, cancellationToken);
                    outboxMessage.MarkAsProcessed();

                    _logger.LogInformation(
                        "Published outbox message {MessageId} of type {Type}",
                        outboxMessage.Id,
                        outboxMessage.Type
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing outbox message {MessageId}",
                    outboxMessage.Id
                );
                outboxMessage.MarkAsFailed(ex.Message);
            }
        }

        if (outboxMessages.Any())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
