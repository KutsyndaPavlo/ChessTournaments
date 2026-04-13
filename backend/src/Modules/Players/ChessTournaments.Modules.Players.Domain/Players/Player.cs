using CSharpFunctionalExtensions;
using BaseEntity = ChessTournaments.Shared.Domain.Entities.Entity;

namespace ChessTournaments.Modules.Players.Domain.Players;

public class Player : BaseEntity
{
    public string UserId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? Country { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }

    // Rating
    public int Rating { get; private set; }
    public int PeakRating { get; private set; }
    public DateTime? PeakRatingDate { get; private set; }

    // Statistics
    public int TotalGamesPlayed { get; private set; }
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Draws { get; private set; }
    public int TournamentsParticipated { get; private set; }
    public int TournamentsWon { get; private set; }

    private Player() { }

    public static Result<Player> Create(
        string userId,
        string firstName,
        string lastName,
        int initialRating = 1200,
        string? country = null,
        DateTime? dateOfBirth = null
    )
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<Player>("User ID is required");

        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<Player>("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<Player>("Last name is required");

        if (initialRating < 0 || initialRating > 3000)
            return Result.Failure<Player>("Initial rating must be between 0 and 3000");

        var player = new Player
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Country = country,
            DateOfBirth = dateOfBirth,
            Rating = initialRating,
            PeakRating = initialRating,
            PeakRatingDate = DateTime.UtcNow,
            TotalGamesPlayed = 0,
            Wins = 0,
            Losses = 0,
            Draws = 0,
            TournamentsParticipated = 0,
            TournamentsWon = 0,
        };

        return Result.Success(player);
    }

    public Result UpdateProfile(
        string firstName,
        string lastName,
        string? country = null,
        DateTime? dateOfBirth = null,
        string? bio = null,
        string? avatarUrl = null
    )
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure("Last name is required");

        FirstName = firstName;
        LastName = lastName;
        Country = country;
        DateOfBirth = dateOfBirth;
        Bio = bio;
        AvatarUrl = avatarUrl;

        MarkAsUpdated();
        return Result.Success();
    }

    public Result UpdateRating(int newRating)
    {
        if (newRating < 0 || newRating > 3000)
            return Result.Failure("Rating must be between 0 and 3000");

        Rating = newRating;

        if (newRating > PeakRating)
        {
            PeakRating = newRating;
            PeakRatingDate = DateTime.UtcNow;
        }

        MarkAsUpdated();
        return Result.Success();
    }

    public void RecordGameResult(bool won, bool draw)
    {
        TotalGamesPlayed++;

        if (won)
            Wins++;
        else if (draw)
            Draws++;
        else
            Losses++;

        MarkAsUpdated();
    }

    public void RecordTournamentParticipation(bool won)
    {
        TournamentsParticipated++;

        if (won)
            TournamentsWon++;

        MarkAsUpdated();
    }

    public string FullName => $"{FirstName} {LastName}";

    public decimal WinRate =>
        TotalGamesPlayed > 0 ? Math.Round((decimal)Wins / TotalGamesPlayed * 100, 2) : 0;

    public decimal DrawRate =>
        TotalGamesPlayed > 0 ? Math.Round((decimal)Draws / TotalGamesPlayed * 100, 2) : 0;

    public decimal LossRate =>
        TotalGamesPlayed > 0 ? Math.Round((decimal)Losses / TotalGamesPlayed * 100, 2) : 0;
}
