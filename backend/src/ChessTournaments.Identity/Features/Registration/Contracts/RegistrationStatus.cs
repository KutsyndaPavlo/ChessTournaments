namespace ChessTournaments.Identity.Features.Registration.Contracts;

public enum RegistrationStatus
{
    Succeeded,
    UserAlreadyExists,
    ValidationError,
    Failed,
}
