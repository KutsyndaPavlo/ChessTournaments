using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Shared.Domain.Enums;

namespace ChessTournaments.Modules.Tournaments.Domain.Tournaments;

public record TournamentSettings
{
    public TournamentFormat Format { get; init; }
    public TimeControl TimeControl { get; init; }
    public int TimeInMinutes { get; init; }
    public int IncrementInSeconds { get; init; }
    public int NumberOfRounds { get; init; }
    public int MaxPlayers { get; init; }
    public int MinPlayers { get; init; }
    public bool AllowByes { get; init; }
    public decimal EntryFee { get; init; }

    public TournamentSettings(
        TournamentFormat format,
        TimeControl timeControl,
        int timeInMinutes,
        int incrementInSeconds,
        int numberOfRounds,
        int maxPlayers,
        int minPlayers,
        bool allowByes,
        decimal entryFee
    )
    {
        if (timeInMinutes <= 0)
            throw new ArgumentException("Time must be greater than zero", nameof(timeInMinutes));

        if (incrementInSeconds < 0)
            throw new ArgumentException("Increment cannot be negative", nameof(incrementInSeconds));

        if (numberOfRounds <= 0)
            throw new ArgumentException(
                "Number of rounds must be greater than zero",
                nameof(numberOfRounds)
            );

        if (maxPlayers < minPlayers)
            throw new ArgumentException(
                "Max players cannot be less than min players",
                nameof(maxPlayers)
            );

        if (minPlayers < 2)
            throw new ArgumentException("Minimum players must be at least 2", nameof(minPlayers));

        Format = format;
        TimeControl = timeControl;
        TimeInMinutes = timeInMinutes;
        IncrementInSeconds = incrementInSeconds;
        NumberOfRounds = numberOfRounds;
        MaxPlayers = maxPlayers;
        MinPlayers = minPlayers;
        AllowByes = allowByes;
        EntryFee = entryFee;
    }
}
