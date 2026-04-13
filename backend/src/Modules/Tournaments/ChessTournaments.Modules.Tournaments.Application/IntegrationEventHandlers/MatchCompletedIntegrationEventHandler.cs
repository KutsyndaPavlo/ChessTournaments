using ChessTournaments.Modules.Matches.IntegrationEvents;
using ChessTournaments.Modules.Tournaments.Application.Features.UpdateMatchResult;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.IntegrationEventHandlers;

/// <summary>
/// Handles MatchCompletedIntegrationEvent from the Matches module
/// </summary>
public class MatchCompletedIntegrationEventHandler
    : INotificationHandler<MatchCompletedIntegrationEvent>
{
    private readonly ISender _sender;

    public MatchCompletedIntegrationEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(
        MatchCompletedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateMatchResultCommand(
            notification.TournamentId,
            notification.RoundId,
            notification.MatchId,
            notification.WhitePlayerId,
            notification.BlackPlayerId,
            notification.Result
        );

        await _sender.Send(command, cancellationToken);
    }
}
