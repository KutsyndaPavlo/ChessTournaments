using ChessTournaments.Modules.Matches.Domain.Matches;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.RemoveMatchTag;

public class RemoveMatchTagCommandHandler : IRequestHandler<RemoveMatchTagCommand, Result>
{
    private readonly IMatchRepository _matchRepository;

    public RemoveMatchTagCommandHandler(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<Result> Handle(
        RemoveMatchTagCommand request,
        CancellationToken cancellationToken
    )
    {
        var match = await _matchRepository.GetByIdAsync(request.MatchId, cancellationToken);

        if (match == null)
            return Result.Failure("Match not found");

        var removeTagResult = match.RemoveTag(request.TagName);

        if (removeTagResult.IsFailure)
            return removeTagResult;

        await _matchRepository.UpdateAsync(match, cancellationToken);
        await _matchRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
