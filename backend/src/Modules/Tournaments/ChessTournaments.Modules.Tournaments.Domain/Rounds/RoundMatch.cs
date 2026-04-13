namespace ChessTournaments.Modules.Tournaments.Domain.Rounds;

/// <summary>
/// Value object representing a match reference in a round
/// The actual match is managed by the Matches module
/// </summary>
public class RoundMatch
{
    public Guid MatchId { get; private set; }
    public int BoardNumber { get; private set; }
    public bool IsCompleted { get; private set; }

    private RoundMatch() { }

    private RoundMatch(Guid matchId, int boardNumber)
    {
        MatchId = matchId;
        BoardNumber = boardNumber;
        IsCompleted = false;
    }

    public static RoundMatch Create(Guid matchId, int boardNumber)
    {
        return new RoundMatch(matchId, boardNumber);
    }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
    }
}
