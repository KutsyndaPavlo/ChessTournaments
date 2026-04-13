namespace ChessTournaments.Modules.Matches.Domain.Common;

public static class DomainErrors
{
    public static class Match
    {
        public static Error RoundIdRequired => new("Match.RoundIdRequired", "Round ID is required");
        public static Error TournamentIdRequired =>
            new("Match.TournamentIdRequired", "Tournament ID is required");
        public static Error WhitePlayerIdRequired =>
            new("Match.WhitePlayerIdRequired", "White player ID is required");
        public static Error BlackPlayerIdRequired =>
            new("Match.BlackPlayerIdRequired", "Black player ID is required");
        public static Error InvalidBoardNumber =>
            new("Match.InvalidBoardNumber", "Board number must be greater than 0");
        public static Error MatchAlreadyCompleted =>
            new("Match.AlreadyCompleted", "Match is already completed");
        public static Error NotFound => new("Match.NotFound", "Match not found");
    }
}

public record Error(string Code, string Message);
