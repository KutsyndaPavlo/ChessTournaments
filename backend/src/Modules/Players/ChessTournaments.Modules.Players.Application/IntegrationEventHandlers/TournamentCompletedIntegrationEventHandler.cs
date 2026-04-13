using ChessTournaments.Modules.Players.Domain.Achievements;
using ChessTournaments.Modules.Players.Domain.Players;
using ChessTournaments.Modules.Tournaments.IntegrationEvents;
using MediatR;

namespace ChessTournaments.Modules.Players.Application.IntegrationEventHandlers;

/// <summary>
/// Handles tournament completion events to update player tournament statistics and save achievements
/// Updates: Tournaments Participated, Tournaments Won (for 1st place), Achievements (top 3)
/// </summary>
public class TournamentCompletedIntegrationEventHandler
    : INotificationHandler<TournamentCompletedIntegrationEvent>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IAchievementRepository _achievementRepository;

    public TournamentCompletedIntegrationEventHandler(
        IPlayerRepository playerRepository,
        IAchievementRepository achievementRepository
    )
    {
        _playerRepository = playerRepository;
        _achievementRepository = achievementRepository;
    }

    public async Task Handle(
        TournamentCompletedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        // Get all players mentioned in the top winners
        var playerIds = notification.TopWinners.Select(w => w.PlayerId).ToList();

        // Process each winner
        foreach (var winner in notification.TopWinners)
        {
            var player = await _playerRepository.GetByUserIdAsync(
                winner.PlayerId,
                cancellationToken
            );

            if (player == null)
            {
                // Player not found - skip
                continue;
            }

            // Record tournament participation
            // Only the 1st place winner gets marked as "won"
            bool wonTournament = winner.Position == 1;
            player.RecordTournamentParticipation(wonTournament);

            await _playerRepository.UpdateAsync(player, cancellationToken);

            // Create achievement record
            // Check if achievement already exists for this tournament and player
            var existingAchievement = await _achievementRepository.GetByTournamentAndPlayerAsync(
                notification.TournamentId,
                player.Id,
                cancellationToken
            );

            if (existingAchievement == null)
            {
                var achievementResult = Achievement.Create(
                    playerId: player.Id,
                    tournamentId: notification.TournamentId,
                    tournamentName: notification.TournamentName,
                    position: winner.Position,
                    score: winner.Score,
                    achievedAt: notification.CompletedAt
                );

                if (achievementResult.IsSuccess)
                {
                    await _achievementRepository.AddAsync(
                        achievementResult.Value,
                        cancellationToken
                    );
                }
            }
        }

        await _playerRepository.SaveChangesAsync(cancellationToken);
        await _achievementRepository.SaveChangesAsync(cancellationToken);
    }
}
