namespace ChessTournaments.Modules.Tournaments.Domain.TournamentPlayers;

public record Score
{
    public decimal Points { get; init; }

    public Score(decimal points)
    {
        if (points < 0)
            throw new ArgumentException("Points cannot be negative", nameof(points));

        Points = points;
    }

    public static Score Win => new(1.0m);
    public static Score Draw => new(0.5m);
    public static Score Loss => new(0.0m);

    public static Score operator +(Score a, Score b) => new(a.Points + b.Points);
}
