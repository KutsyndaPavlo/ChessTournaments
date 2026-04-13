using MediatR;

namespace ChessTournaments.Shared.Domain.Events;

/// <summary>
/// Represents a domain event that can be published and handled by domain event handlers.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the UTC date and time when this event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
