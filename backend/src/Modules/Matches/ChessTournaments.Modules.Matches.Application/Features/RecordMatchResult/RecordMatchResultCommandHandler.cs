using ChessTournaments.Modules.Matches.Domain.Matches;
using ChessTournaments.Modules.Matches.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.Matches.Application.Features.RecordMatchResult;

public class RecordMatchResultCommandHandler : IRequestHandler<RecordMatchResultCommand, Result>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IOutboxMessagePublisher _outboxPublisher;

    public RecordMatchResultCommandHandler(
        IMatchRepository matchRepository,
        [FromKeyedServices("MatchesDbContext")] IOutboxMessagePublisher outboxPublisher
    )
    {
        _matchRepository = matchRepository;
        _outboxPublisher = outboxPublisher;
    }

    public async Task<Result> Handle(
        RecordMatchResultCommand request,
        CancellationToken cancellationToken
    )
    {
        var match = await _matchRepository.GetByIdAsync(request.MatchId, cancellationToken);

        if (match == null)
            return Result.Failure("Match not found");

        var recordResult = match.RecordResult(request.Result, request.Moves);

        if (recordResult.IsFailure)
            return recordResult;

        await _matchRepository.UpdateAsync(match, cancellationToken);

        // Publish integration event to notify other modules
        // IMPORTANT: Publish BEFORE SaveChanges to ensure event is saved in same transaction
        var integrationEvent = new MatchCompletedIntegrationEvent(
            match.Id,
            match.TournamentId,
            match.RoundId,
            match.WhitePlayerId,
            match.BlackPlayerId,
            (int)match.Result, // Cast enum to int to avoid cross-module dependencies
            match.CompletedAt!.Value
        );

        await _outboxPublisher.PublishAsync(integrationEvent, cancellationToken);

        // Save both domain changes and outbox message in single transaction
        await _matchRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
