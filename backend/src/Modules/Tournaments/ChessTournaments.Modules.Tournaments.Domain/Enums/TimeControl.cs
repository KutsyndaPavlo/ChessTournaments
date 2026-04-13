namespace ChessTournaments.Modules.Tournaments.Domain.Enums;

public enum TimeControl
{
    Bullet = 0, // < 3 minutes
    Blitz = 1, // 3-10 minutes
    Rapid = 2, // 10-60 minutes
    Classical = 3, // > 60 minutes
}
