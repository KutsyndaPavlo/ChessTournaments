namespace ChessTournaments.Identity.Configurations;

public class IdentitySettings
{
    public int MaxFailedPasswordCount { get; set; }

    public int DefaultLockoutTimeSpanInHours { get; set; }
}
