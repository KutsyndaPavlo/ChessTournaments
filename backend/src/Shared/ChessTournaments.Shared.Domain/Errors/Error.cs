namespace ChessTournaments.Shared.Domain.Errors;

/// <summary>
/// Represents a domain error with a code and message.
/// </summary>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Represents no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}
