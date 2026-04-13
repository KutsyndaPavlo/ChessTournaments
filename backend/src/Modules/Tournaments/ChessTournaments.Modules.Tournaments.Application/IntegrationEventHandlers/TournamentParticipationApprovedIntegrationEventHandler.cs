using ChessTournaments.Shared.IntegrationEvents;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.IntegrationEventHandlers;

/// <summary>
/// Handles tournament participation approval by registering the player
/// This handler is in the Tournaments module and listens to events from TournamentRequests module
/// </summary>
public class TournamentParticipationApprovedIntegrationEventHandler
    : INotificationHandler<TournamentParticipationApprovedIntegrationEvent>
{
    private readonly ISender _sender;

    public TournamentParticipationApprovedIntegrationEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(
        TournamentParticipationApprovedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        // Get player information from Players module
        var getPlayerQuery =
            new Players.Application.Features.GetPlayerByUserId.GetPlayerByUserIdQuery(
                notification.PlayerId
            );
        var playerResult = await _sender.Send(getPlayerQuery, cancellationToken);

        if (playerResult.IsFailure)
        {
            // Log error or throw exception based on your error handling strategy
            throw new InvalidOperationException(
                $"Failed to get player information for {notification.PlayerId}: {playerResult.Error}"
            );
        }

        var player = playerResult.Value;

        // Register the player in the tournament
        var registerPlayerCommand = new Features.RegisterPlayer.RegisterPlayerCommand(
            notification.TournamentId,
            player.UserId,
            player.FullName,
            player.Rating
        );

        var registerResult = await _sender.Send(registerPlayerCommand, cancellationToken);

        if (registerResult.IsFailure)
        {
            // Log error or throw exception based on your error handling strategy
            throw new InvalidOperationException(
                $"Failed to register player {notification.PlayerId} in tournament {notification.TournamentId}: {registerResult.Error}"
            );
        }
    }
}
