using FluentValidation;

namespace ChessTournaments.Modules.Tournaments.Application.Features.UpdateTournament;

public class UpdateTournamentCommandValidator : AbstractValidator<UpdateTournamentCommand>
{
    public UpdateTournamentCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
