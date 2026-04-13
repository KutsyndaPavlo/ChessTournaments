using FluentValidation;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CreateTournament;

public class CreateTournamentCommandValidator : AbstractValidator<CreateTournamentCommand>
{
    public CreateTournamentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.StartDate).GreaterThan(DateTime.UtcNow.AddDays(-1));
        RuleFor(x => x.OrganizerId).NotEmpty();
        RuleFor(x => x.TimeInMinutes).GreaterThan(0);
        RuleFor(x => x.IncrementInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.NumberOfRounds).GreaterThan(0).LessThanOrEqualTo(20);
        RuleFor(x => x.MaxPlayers).GreaterThan(1).LessThanOrEqualTo(1000);
        RuleFor(x => x.MinPlayers).GreaterThan(1).LessThan(x => x.MaxPlayers);
        RuleFor(x => x.EntryFee).GreaterThanOrEqualTo(0);
    }
}
