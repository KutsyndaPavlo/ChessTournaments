using ChessTournaments.Modules.Matches.Application.Features.CreateMatch;
using ChessTournaments.Modules.Tournaments.IntegrationEvents;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.IntegrationEventHandlers;

/// <summary>
/// Handles match creation requests from the Tournaments module
/// Creates the match and publishes MatchCreatedIntegrationEvent
/// </summary>
public class CreateMatchRequestedIntegrationEventHandler
    : INotificationHandler<CreateMatchRequestedIntegrationEvent>
{
    private readonly ISender _sender;

    public CreateMatchRequestedIntegrationEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(
        CreateMatchRequestedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateMatchCommand(
            notification.RoundId,
            notification.TournamentId,
            notification.WhitePlayerId,
            notification.BlackPlayerId,
            notification.BoardNumber
        );

        // Create the match - the handler will publish MatchCreatedIntegrationEvent
        await _sender.Send(command, cancellationToken);
    }
}
