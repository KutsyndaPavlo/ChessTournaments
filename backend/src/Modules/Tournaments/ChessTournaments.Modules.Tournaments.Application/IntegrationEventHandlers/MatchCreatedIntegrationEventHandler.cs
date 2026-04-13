using ChessTournaments.Modules.Matches.IntegrationEvents;
using ChessTournaments.Modules.Tournaments.Application.Features.AddMatchReference;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.IntegrationEventHandlers;

/// <summary>
/// Handles MatchCreatedIntegrationEvent from the Matches module
/// </summary>
public class MatchCreatedIntegrationEventHandler
    : INotificationHandler<MatchCreatedIntegrationEvent>
{
    private readonly ISender _sender;

    public MatchCreatedIntegrationEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(
        MatchCreatedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        var command = new AddMatchReferenceCommand(
            notification.TournamentId,
            notification.RoundId,
            notification.MatchId,
            notification.BoardNumber
        );

        await _sender.Send(command, cancellationToken);
    }
}
