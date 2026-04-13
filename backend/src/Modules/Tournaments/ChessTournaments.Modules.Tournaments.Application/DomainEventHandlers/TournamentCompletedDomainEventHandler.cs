using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Modules.Tournaments.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.Tournaments.Application.DomainEventHandlers;

public class TournamentCompletedDomainEventHandler
    : INotificationHandler<TournamentCompletedDomainEvent>
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly IOutboxMessagePublisher _outboxPublisher;

    public TournamentCompletedDomainEventHandler(
        ITournamentRepository tournamentRepository,
        [FromKeyedServices("TournamentsDbContext")] IOutboxMessagePublisher outboxPublisher
    )
    {
        _tournamentRepository = tournamentRepository;
        _outboxPublisher = outboxPublisher;
    }

    public async Task Handle(
        TournamentCompletedDomainEvent notification,
        CancellationToken cancellationToken
    )
    {
        // Fetch the tournament with players to get final standings
        var tournament = await _tournamentRepository.GetByIdWithPlayersAndRoundsAsync(
            notification.TournamentId,
            cancellationToken
        );

        if (tournament == null)
        {
            // Tournament not found - should not happen but handle gracefully
            return;
        }

        // Get top 3 players sorted by score (descending)
        var topPlayers = tournament
            .Players.OrderByDescending(p => p.TotalScore.Points)
            .Take(3)
            .Select(
                (player, index) =>
                    new WinnerInfo(
                        Position: index + 1,
                        PlayerId: player.PlayerId,
                        PlayerName: player.PlayerName,
                        Score: player.TotalScore.Points
                    )
            )
            .ToList();

        // Publish integration event
        var integrationEvent = new TournamentCompletedIntegrationEvent(
            TournamentId: tournament.Id,
            TournamentName: tournament.Name,
            CompletedAt: tournament.EndDate ?? DateTime.UtcNow,
            TopWinners: topPlayers
        );

        await _outboxPublisher.PublishAsync(integrationEvent, cancellationToken);
    }
}
