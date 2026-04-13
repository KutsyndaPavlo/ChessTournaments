using ChessTournaments.Modules.Matches.Domain.Common;
using CSharpFunctionalExtensions;
using BaseEntity = ChessTournaments.Shared.Domain.Entities.Entity;

namespace ChessTournaments.Modules.Matches.Domain.Matches;

public class MatchTag : BaseEntity
{
    public Guid MatchId { get; private set; }
    public string Name { get; private set; }

    private MatchTag()
    {
        // For EF
    }

    private MatchTag(Guid matchId, string name)
    {
        MatchId = matchId;
        Name = name;
    }

    public static CSharpFunctionalExtensions.Result<MatchTag> Create(Guid matchId, string name)
    {
        if (matchId == Guid.Empty)
            return CSharpFunctionalExtensions.Result.Failure<MatchTag>("Match ID is required");

        if (string.IsNullOrWhiteSpace(name))
            return CSharpFunctionalExtensions.Result.Failure<MatchTag>("Tag name is required");

        if (name.Length > 50)
            return CSharpFunctionalExtensions.Result.Failure<MatchTag>(
                "Tag name must be 50 characters or less"
            );

        var tag = new MatchTag(matchId, name.Trim());
        return CSharpFunctionalExtensions.Result.Success(tag);
    }
}
