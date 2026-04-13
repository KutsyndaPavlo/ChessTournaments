using ChessTournaments.Modules.Matches.Domain.Matches;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.UpdateMatchMoves;

public class UpdateMatchMovesCommandHandler : IRequestHandler<UpdateMatchMovesCommand, Result>
{
    private readonly IMatchRepository _matchRepository;

    public UpdateMatchMovesCommandHandler(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<Result> Handle(
        UpdateMatchMovesCommand request,
        CancellationToken cancellationToken
    )
    {
        var match = await _matchRepository.GetByIdAsync(request.MatchId, cancellationToken);

        if (match == null)
            return Result.Failure("Match not found");

        var updateResult = match.UpdateMoves(request.Moves);

        if (updateResult.IsFailure)
            return updateResult;

        await _matchRepository.UpdateAsync(match, cancellationToken);
        await _matchRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
