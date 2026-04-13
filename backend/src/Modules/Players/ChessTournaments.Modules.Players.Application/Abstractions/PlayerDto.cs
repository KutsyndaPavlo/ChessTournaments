namespace ChessTournaments.Modules.Players.Application.Abstractions;

public record PlayerDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? Country { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public int Rating { get; init; }
    public int PeakRating { get; init; }
    public DateTime? PeakRatingDate { get; init; }
    public int TotalGamesPlayed { get; init; }
    public int Wins { get; init; }
    public int Losses { get; init; }
    public int Draws { get; init; }
    public int TournamentsParticipated { get; init; }
    public int TournamentsWon { get; init; }
    public decimal WinRate { get; init; }
    public decimal DrawRate { get; init; }
    public decimal LossRate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
