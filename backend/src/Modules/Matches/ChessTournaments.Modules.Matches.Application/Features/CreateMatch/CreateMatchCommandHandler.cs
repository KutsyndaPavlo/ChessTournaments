using ChessTournaments.Modules.Matches.Domain.Matches;
using ChessTournaments.Modules.Matches.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.Matches.Application.Features.CreateMatch;

public class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, Result<Guid>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IOutboxMessagePublisher _outboxPublisher;

    public CreateMatchCommandHandler(
        IMatchRepository matchRepository,
        [FromKeyedServices("MatchesDbContext")] IOutboxMessagePublisher outboxPublisher
    )
    {
        _matchRepository = matchRepository;
        _outboxPublisher = outboxPublisher;
    }

    public async Task<Result<Guid>> Handle(
        CreateMatchCommand request,
        CancellationToken cancellationToken
    )
    {
        var matchResult = Match.Create(
            request.RoundId,
            request.TournamentId,
            request.WhitePlayerId,
            request.BlackPlayerId,
            request.BoardNumber
        );

        if (matchResult.IsFailure)
            return Result.Failure<Guid>(matchResult.Error);

        var match = matchResult.Value;

        await _matchRepository.AddAsync(match, cancellationToken);

        // Publish integration event to notify other modules
        // IMPORTANT: Publish BEFORE SaveChanges to ensure event is saved in same transaction
        var integrationEvent = new MatchCreatedIntegrationEvent(
            match.Id,
            match.TournamentId,
            match.RoundId,
            match.WhitePlayerId,
            match.BlackPlayerId,
            match.BoardNumber
        );

        await _outboxPublisher.PublishAsync(integrationEvent, cancellationToken);

        // Save both domain changes and outbox message in single transaction
        await _matchRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(match.Id);
    }
}
