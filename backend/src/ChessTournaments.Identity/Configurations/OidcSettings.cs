namespace ChessTournaments.Identity.Configurations;

public class OidcSettings
{
    public string Issuer { get; set; } = string.Empty;

    public int AccessTokenLifetimeHours { get; set; }

    public int RefreshTokenLifetimeHours { get; set; }

    public ApiClientSettings API { get; set; } = new();

    public SpaClientSettings SPA { get; set; } = new();

    public List<CustomScope> CustomScopes { get; set; } = new();
}

public class ApiClientSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class SpaClientSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> LoginCallbackUris { get; set; } = new();
    public List<string> LogoutCallbackUris { get; set; } = new();
}

public class CustomScope
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
