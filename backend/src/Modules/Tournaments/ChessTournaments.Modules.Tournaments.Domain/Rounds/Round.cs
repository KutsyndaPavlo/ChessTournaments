using ChessTournaments.Modules.Tournaments.Domain.Common;
using CSharpFunctionalExtensions;
using BaseEntity = ChessTournaments.Shared.Domain.Entities.Entity;

namespace ChessTournaments.Modules.Tournaments.Domain.Rounds;

/// <summary>
/// Round aggregate - now only tracks match IDs
/// Actual match management is delegated to the Matches module
/// </summary>
public class Round : BaseEntity
{
    private readonly List<RoundMatch> _matchReferences = [];

    public Guid TournamentId { get; private set; }
    public int RoundNumber { get; private set; }
    public DateTime? StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public bool IsCompleted { get; private set; }

    public IReadOnlyCollection<RoundMatch> MatchReferences => _matchReferences.AsReadOnly();

    // Helper property for compatibility
    public int MatchCount => _matchReferences.Count;

    private Round()
    {
        // For EF
    }

    private Round(Guid tournamentId, int roundNumber)
    {
        TournamentId = tournamentId;
        RoundNumber = roundNumber;
        IsCompleted = false;
    }

    public static Result<Round> Create(Guid tournamentId, int roundNumber)
    {
        if (tournamentId == Guid.Empty)
            return Result.Failure<Round>(DomainErrors.Round.TournamentIdRequired.Message);

        if (roundNumber <= 0)
            return Result.Failure<Round>(DomainErrors.Round.InvalidRoundNumber.Message);

        var round = new Round(tournamentId, roundNumber);
        return Result.Success(round);
    }

    public Result Start()
    {
        if (StartTime.HasValue)
            return Result.Failure("Round has already started");

        StartTime = DateTime.UtcNow;
        MarkAsUpdated();
        AddDomainEvent(new RoundStartedDomainEvent(TournamentId, Id, RoundNumber));

        return Result.Success();
    }

    /// <summary>
    /// Adds a match reference to the round
    /// The actual match should be created in the Matches module first
    /// </summary>
    public Result AddMatchReference(Guid matchId, int boardNumber)
    {
        if (IsCompleted)
            return Result.Failure("Cannot add match to completed round");

        if (_matchReferences.Any(m => m.BoardNumber == boardNumber))
            return Result.Failure($"Board {boardNumber} already has a match");

        if (_matchReferences.Any(m => m.MatchId == matchId))
            return Result.Failure("Match already exists in this round");

        var matchRef = RoundMatch.Create(matchId, boardNumber);
        _matchReferences.Add(matchRef);
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Marks a match as completed (called when integration event is received)
    /// </summary>
    public Result MarkMatchCompleted(Guid matchId)
    {
        var matchRef = _matchReferences.FirstOrDefault(m => m.MatchId == matchId);
        if (matchRef == null)
            return Result.Failure("Match not found in this round");

        matchRef.MarkAsCompleted();
        MarkAsUpdated();

        return Result.Success();
    }

    public Result Complete()
    {
        if (IsCompleted)
            return Result.Failure("Round is already completed");

        if (_matchReferences.Any(m => !m.IsCompleted))
            return Result.Failure("Cannot complete round with ongoing matches");

        IsCompleted = true;
        EndTime = DateTime.UtcNow;
        MarkAsUpdated();
        AddDomainEvent(new RoundCompletedDomainEvent(TournamentId, Id, RoundNumber));

        return Result.Success();
    }
}
