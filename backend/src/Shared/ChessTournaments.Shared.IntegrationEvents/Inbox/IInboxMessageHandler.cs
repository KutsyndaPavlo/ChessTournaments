namespace ChessTournaments.Shared.IntegrationEvents.Inbox;

/// <summary>
/// Handles integration events using the Inbox pattern for idempotent processing
/// </summary>
public interface IInboxMessageHandler
{
    Task<bool> TryProcessAsync(
        Guid messageId,
        string messageType,
        string content,
        CancellationToken cancellationToken = default
    );
}
