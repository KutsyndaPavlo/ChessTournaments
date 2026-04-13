namespace ChessTournaments.Identity.Features.Login.Contracts;

public sealed class LoginResponse
{
    public LoginStatus Status { get; init; }

    public string? Message { get; init; }
}
