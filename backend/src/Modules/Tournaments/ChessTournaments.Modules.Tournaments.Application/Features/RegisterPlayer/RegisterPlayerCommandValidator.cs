using FluentValidation;

namespace ChessTournaments.Modules.Tournaments.Application.Features.RegisterPlayer;

public class RegisterPlayerCommandValidator : AbstractValidator<RegisterPlayerCommand>
{
    public RegisterPlayerCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.PlayerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Rating).GreaterThanOrEqualTo(0).When(x => x.Rating.HasValue);
    }
}
