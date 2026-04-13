using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Domain.Common;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Modules.Tournaments.IntegrationEvents;
using ChessTournaments.Shared.Domain.Enums;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CreateRoundWithPairings;

public class CreateRoundWithPairingsCommandHandler
    : IRequestHandler<CreateRoundWithPairingsCommand, Result<RoundDto>>
{
    private readonly ITournamentRepository _repository;
    private readonly IOutboxMessagePublisher _outboxPublisher;

    public CreateRoundWithPairingsCommandHandler(
        ITournamentRepository repository,
        [FromKeyedServices("TournamentsDbContext")] IOutboxMessagePublisher outboxPublisher
    )
    {
        _repository = repository;
        _outboxPublisher = outboxPublisher;
    }

    public async Task<Result<RoundDto>> Handle(
        CreateRoundWithPairingsCommand request,
        CancellationToken cancellationToken
    )
    {
        var tournament = await _repository.GetByIdWithPlayersAndRoundsAsync(
            request.TournamentId,
            cancellationToken
        );

        if (tournament == null)
            return Result.Failure<RoundDto>(DomainErrors.Tournament.NotFound.Message);

        if (tournament.Status != TournamentStatus.InProgress)
            return Result.Failure<RoundDto>("Tournament must be in progress to create rounds");

        var currentRoundNumber = tournament.Rounds.Count + 1;

        if (currentRoundNumber > tournament.Settings.NumberOfRounds)
            return Result.Failure<RoundDto>("All rounds have been created");

        // Check if previous round is completed
        if (tournament.Rounds.Any() && tournament.Rounds.Any(r => !r.IsCompleted))
            return Result.Failure<RoundDto>("Previous round must be completed first");

        var roundResult = tournament.CreateRound(currentRoundNumber);

        if (roundResult.IsFailure)
            return Result.Failure<RoundDto>(roundResult.Error);

        var round = roundResult.Value;

        // Generate pairings based on tournament format
        var pairingsResult = await GeneratePairingsAsync(tournament, round, cancellationToken);

        if (pairingsResult.IsFailure)
            return Result.Failure<RoundDto>(pairingsResult.Error);

        await _repository.UpdateAsync(tournament, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new RoundDto
        {
            Id = round.Id,
            TournamentId = tournament.Id,
            RoundNumber = round.RoundNumber,
            StartTime = round.StartTime,
            EndTime = round.EndTime,
            IsCompleted = round.IsCompleted,
            MatchCount = round.MatchCount,
            Matches = new List<MatchDto>(), // Match details should be fetched from Matches module
        };

        return Result.Success(dto);
    }

    private async Task<Result> GeneratePairingsAsync(
        Tournament tournament,
        Domain.Rounds.Round round,
        CancellationToken cancellationToken
    )
    {
        var players = tournament.Players.ToList();

        if (players.Count < 2)
            return Result.Failure("Not enough players to create pairings");

        // Simple Swiss pairing: sort by score (descending), then rating (descending)
        var sortedPlayers = players
            .OrderByDescending(p => p.TotalScore.Points)
            .ThenByDescending(p => p.Rating ?? 0)
            .ToList();

        int boardNumber = 1;
        var paired = new HashSet<string>();

        for (int i = 0; i < sortedPlayers.Count - 1; i++)
        {
            if (paired.Contains(sortedPlayers[i].PlayerId))
                continue;

            // Find opponent: first unpaired player from the remaining list
            for (int j = i + 1; j < sortedPlayers.Count; j++)
            {
                if (paired.Contains(sortedPlayers[j].PlayerId))
                    continue;

                // Note: Cannot check if players have played before since we don't have access to match details
                // This would require querying the Matches module, which adds complexity
                // For now, we'll skip this check and create the pairing

                // Request match creation via integration event
                // The Matches module will create the match and publish MatchCreatedIntegrationEvent
                // The match reference will be added to the round via MatchCreatedIntegrationEventHandler
                var createMatchEvent = new CreateMatchRequestedIntegrationEvent(
                    round.Id,
                    tournament.Id,
                    sortedPlayers[i].PlayerId,
                    sortedPlayers[j].PlayerId,
                    boardNumber
                );

                await _outboxPublisher.PublishAsync(createMatchEvent, cancellationToken);

                paired.Add(sortedPlayers[i].PlayerId);
                paired.Add(sortedPlayers[j].PlayerId);
                boardNumber++;
                break;
            }
        }

        // Handle bye for unpaired player
        if (paired.Count < sortedPlayers.Count)
        {
            var unpairedPlayer = sortedPlayers.FirstOrDefault(p => !paired.Contains(p.PlayerId));
            if (unpairedPlayer != null && tournament.Settings.AllowByes)
            {
                // Award bye point
                unpairedPlayer.UpdateScore(Domain.TournamentPlayers.Score.Win);
            }
        }

        return Result.Success();
    }
}
