namespace ChessTournaments.Shared.Domain.Inbox;

/// <summary>
/// Represents an inbox message for idempotent integration event processing
/// </summary>
public class InboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }

    private InboxMessage() { }

    public InboxMessage(Guid id, string type, string content, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    public void MarkAsProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
        ProcessedOnUtc = DateTime.UtcNow;
    }
}
