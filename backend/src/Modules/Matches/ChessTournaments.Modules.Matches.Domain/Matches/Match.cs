using ChessTournaments.Modules.Matches.Domain.Common;
using ChessTournaments.Shared.Domain.Enums;
using CSharpFunctionalExtensions;
using BaseEntity = ChessTournaments.Shared.Domain.Entities.Entity;

namespace ChessTournaments.Modules.Matches.Domain.Matches;

public class Match : BaseEntity
{
    private readonly List<MatchTag> _tags = [];

    public Guid RoundId { get; private set; }
    public Guid TournamentId { get; private set; }
    public string WhitePlayerId { get; private set; }
    public string BlackPlayerId { get; private set; }
    public int BoardNumber { get; private set; }
    public GameResult Result { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Moves { get; private set; }

    public IReadOnlyCollection<MatchTag> Tags => _tags.AsReadOnly();

    private Match()
    {
        // For EF
    }

    private Match(
        Guid roundId,
        Guid tournamentId,
        string whitePlayerId,
        string blackPlayerId,
        int boardNumber
    )
    {
        RoundId = roundId;
        TournamentId = tournamentId;
        WhitePlayerId = whitePlayerId;
        BlackPlayerId = blackPlayerId;
        BoardNumber = boardNumber;
        Result = GameResult.Ongoing;
        IsCompleted = false;
    }

    public static CSharpFunctionalExtensions.Result<Match> Create(
        Guid roundId,
        Guid tournamentId,
        string whitePlayerId,
        string blackPlayerId,
        int boardNumber
    )
    {
        if (roundId == Guid.Empty)
            return CSharpFunctionalExtensions.Result.Failure<Match>(
                DomainErrors.Match.RoundIdRequired.Message
            );

        if (tournamentId == Guid.Empty)
            return CSharpFunctionalExtensions.Result.Failure<Match>("Tournament ID is required");

        if (string.IsNullOrWhiteSpace(whitePlayerId))
            return CSharpFunctionalExtensions.Result.Failure<Match>(
                DomainErrors.Match.WhitePlayerIdRequired.Message
            );

        if (string.IsNullOrWhiteSpace(blackPlayerId))
            return CSharpFunctionalExtensions.Result.Failure<Match>(
                DomainErrors.Match.BlackPlayerIdRequired.Message
            );

        if (whitePlayerId == blackPlayerId)
            return CSharpFunctionalExtensions.Result.Failure<Match>(
                "White and black players must be different"
            );

        if (boardNumber <= 0)
            return CSharpFunctionalExtensions.Result.Failure<Match>(
                DomainErrors.Match.InvalidBoardNumber.Message
            );

        var match = new Match(roundId, tournamentId, whitePlayerId, blackPlayerId, boardNumber);
        return CSharpFunctionalExtensions.Result.Success(match);
    }

    public CSharpFunctionalExtensions.Result RecordResult(GameResult result, string? moves = null)
    {
        if (IsCompleted)
            return CSharpFunctionalExtensions.Result.Failure(
                DomainErrors.Match.MatchAlreadyCompleted.Message
            );

        if (result == GameResult.Ongoing)
            return CSharpFunctionalExtensions.Result.Failure("Cannot record ongoing as a result");

        Result = result;
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        Moves = moves;
        MarkAsUpdated();

        AddDomainEvent(
            new MatchCompletedDomainEvent(
                TournamentId,
                RoundId,
                Id,
                WhitePlayerId,
                BlackPlayerId,
                result
            )
        );

        return CSharpFunctionalExtensions.Result.Success();
    }

    public CSharpFunctionalExtensions.Result UpdateMoves(string moves)
    {
        if (IsCompleted)
            return CSharpFunctionalExtensions.Result.Failure(
                "Cannot update moves for completed match"
            );

        Moves = moves;
        MarkAsUpdated();

        return CSharpFunctionalExtensions.Result.Success();
    }

    public CSharpFunctionalExtensions.Result AddTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return CSharpFunctionalExtensions.Result.Failure("Tag name cannot be empty");

        if (_tags.Any(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
            return CSharpFunctionalExtensions.Result.Failure($"Tag '{tagName}' already exists");

        var tagResult = MatchTag.Create(Id, tagName);
        if (tagResult.IsFailure)
            return CSharpFunctionalExtensions.Result.Failure(tagResult.Error);

        _tags.Add(tagResult.Value);
        MarkAsUpdated();

        return CSharpFunctionalExtensions.Result.Success();
    }

    public CSharpFunctionalExtensions.Result RemoveTag(string tagName)
    {
        var tag = _tags.FirstOrDefault(t =>
            t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)
        );

        if (tag == null)
            return CSharpFunctionalExtensions.Result.Failure($"Tag '{tagName}' not found");

        _tags.Remove(tag);
        MarkAsUpdated();

        return CSharpFunctionalExtensions.Result.Success();
    }
}
