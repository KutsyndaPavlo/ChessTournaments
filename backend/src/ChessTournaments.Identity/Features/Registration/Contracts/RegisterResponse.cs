namespace ChessTournaments.Identity.Features.Registration.Contracts;

public sealed class RegisterResponse
{
    public RegistrationStatus Status { get; init; }
    public string? Message { get; init; }
    public IEnumerable<string>? Errors { get; init; }

    public static RegisterResponse Success() =>
        new() { Status = RegistrationStatus.Succeeded, Message = "Registration successful" };

    public static RegisterResponse UserExists() =>
        new()
        {
            Status = RegistrationStatus.UserAlreadyExists,
            Message = "Username or email already exists",
        };

    public static RegisterResponse ValidationError(IEnumerable<string> errors) =>
        new()
        {
            Status = RegistrationStatus.ValidationError,
            Message = "Validation failed",
            Errors = errors,
        };

    public static RegisterResponse Failed(string message) =>
        new() { Status = RegistrationStatus.Failed, Message = message };
}
