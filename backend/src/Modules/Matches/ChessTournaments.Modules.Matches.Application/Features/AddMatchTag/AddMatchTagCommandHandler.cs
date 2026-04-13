using ChessTournaments.Modules.Matches.Domain.Matches;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.AddMatchTag;

public class AddMatchTagCommandHandler : IRequestHandler<AddMatchTagCommand, Result>
{
    private readonly IMatchRepository _matchRepository;

    public AddMatchTagCommandHandler(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<Result> Handle(
        AddMatchTagCommand request,
        CancellationToken cancellationToken
    )
    {
        var match = await _matchRepository.GetByIdAsync(request.MatchId, cancellationToken);

        if (match == null)
            return Result.Failure("Match not found");

        var addTagResult = match.AddTag(request.TagName);

        if (addTagResult.IsFailure)
            return addTagResult;

        await _matchRepository.UpdateAsync(match, cancellationToken);
        await _matchRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
