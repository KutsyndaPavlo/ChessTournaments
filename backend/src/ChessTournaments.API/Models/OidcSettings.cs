namespace ChessTournaments.API.Models;

public class OidcSettings
{
    public string Authority { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public ApiSettings API { get; init; } = new();

    public record ApiSettings
    {
        public string ClientId { get; init; } = string.Empty;

        public string ClientSecret { get; init; } = string.Empty;
    }
}
