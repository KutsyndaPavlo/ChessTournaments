namespace ChessTournaments.API.Infrastructure.OpenApi;

internal static class OpenApiMetadata
{
    public static string ApiName => "Chess Tournaments API";

    public static string? ApiVersion => typeof(Program).Assembly.GetName().Version?.ToString();
}
