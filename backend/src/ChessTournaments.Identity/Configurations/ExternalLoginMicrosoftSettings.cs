namespace ChessTournaments.Identity.Configurations;

public class ExternalLoginMicrosoftSettings
{
    public bool Enabled { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? TenantId { get; set; }

    public string[]? ClientIdsRequiringPrompt { get; set; }

    public bool IsSingleTenant => !string.IsNullOrWhiteSpace(TenantId);
}
