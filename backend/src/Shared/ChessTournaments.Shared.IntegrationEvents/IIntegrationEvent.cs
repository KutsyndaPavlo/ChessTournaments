using MediatR;

namespace ChessTournaments.Shared.IntegrationEvents;

/// <summary>
/// Marker interface for integration events that cross module boundaries
/// Inherits from INotification to enable MediatR dispatching
/// </summary>
public interface IIntegrationEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
