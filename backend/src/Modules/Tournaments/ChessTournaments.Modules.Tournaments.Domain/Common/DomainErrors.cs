namespace ChessTournaments.Modules.Tournaments.Domain.Common;

public static class DomainErrors
{
    public static class Tournament
    {
        public static readonly Error NameRequired = new(
            "Tournament.NameRequired",
            "Tournament name is required"
        );
        public static readonly Error StartDateInPast = new(
            "Tournament.StartDateInPast",
            "Start date must be in the future"
        );
        public static readonly Error OrganizerIdRequired = new(
            "Tournament.OrganizerIdRequired",
            "Organizer ID is required"
        );
        public static readonly Error SettingsRequired = new(
            "Tournament.SettingsRequired",
            "Tournament settings are required"
        );
        public static readonly Error CannotUpdateInProgress = new(
            "Tournament.CannotUpdateInProgress",
            "Cannot update details for tournament in progress or completed"
        );
        public static readonly Error RegistrationNotOpen = new(
            "Tournament.RegistrationNotOpen",
            "Registration is not open"
        );
        public static readonly Error TournamentFull = new(
            "Tournament.TournamentFull",
            "Tournament is full"
        );
        public static readonly Error PlayerAlreadyRegistered = new(
            "Tournament.PlayerAlreadyRegistered",
            "Player is already registered"
        );
        public static readonly Error CannotCancelCompleted = new(
            "Tournament.CannotCancelCompleted",
            "Cannot cancel completed tournament"
        );
        public static readonly Error CannotCloseRegistration = new(
            "Tournament.CannotCloseRegistration",
            "Cannot close registration for tournament not accepting registrations"
        );
        public static readonly Error NotFound = new("Tournament.NotFound", "Tournament not found");
        public static readonly Error CannotStartTournament = new(
            "Tournament.CannotStartTournament",
            "Cannot start tournament that is not ready"
        );
        public static readonly Error InsufficientPlayers = new(
            "Tournament.InsufficientPlayers",
            "Not enough players registered"
        );
        public static readonly Error CannotCompleteTournament = new(
            "Tournament.CannotCompleteTournament",
            "Cannot complete tournament that is not in progress"
        );
    }

    public static class TournamentPlayer
    {
        public static readonly Error TournamentIdRequired = new(
            "TournamentPlayer.TournamentIdRequired",
            "Tournament ID is required"
        );
        public static readonly Error PlayerIdRequired = new(
            "TournamentPlayer.PlayerIdRequired",
            "Player ID is required"
        );
        public static readonly Error PlayerNameRequired = new(
            "TournamentPlayer.PlayerNameRequired",
            "Player name is required"
        );
    }

    public static class Round
    {
        public static readonly Error TournamentIdRequired = new(
            "Round.TournamentIdRequired",
            "Tournament ID is required"
        );
        public static readonly Error InvalidRoundNumber = new(
            "Round.InvalidRoundNumber",
            "Round number must be greater than zero"
        );
    }

    public static class Match
    {
        public static readonly Error RoundIdRequired = new(
            "Match.RoundIdRequired",
            "Round ID is required"
        );
        public static readonly Error WhitePlayerIdRequired = new(
            "Match.WhitePlayerIdRequired",
            "White player ID is required"
        );
        public static readonly Error BlackPlayerIdRequired = new(
            "Match.BlackPlayerIdRequired",
            "Black player ID is required"
        );
        public static readonly Error InvalidBoardNumber = new(
            "Match.InvalidBoardNumber",
            "Board number must be greater than zero"
        );
        public static readonly Error MatchAlreadyCompleted = new(
            "Match.MatchAlreadyCompleted",
            "Match is already completed"
        );
    }

    public static class Score
    {
        public static readonly Error InvalidPoints = new(
            "Score.InvalidPoints",
            "Points must be between 0 and 1"
        );
    }
}

public record Error(string Code, string Message);
