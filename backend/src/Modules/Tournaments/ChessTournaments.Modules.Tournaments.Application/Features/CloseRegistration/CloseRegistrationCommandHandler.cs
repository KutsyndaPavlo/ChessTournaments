using ChessTournaments.Modules.Tournaments.Domain.Common;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CloseRegistration;

public class CloseRegistrationCommandHandler : IRequestHandler<CloseRegistrationCommand, Result>
{
    private readonly ITournamentRepository _repository;

    public CloseRegistrationCommandHandler(ITournamentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        CloseRegistrationCommand request,
        CancellationToken cancellationToken
    )
    {
        var tournament = await _repository.GetByIdAsync(request.TournamentId, cancellationToken);

        if (tournament == null)
            return Result.Failure(DomainErrors.Tournament.NotFound.Message);

        var result = tournament.CloseRegistration();

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _repository.UpdateAsync(tournament, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        // Domain event TournamentRegistrationClosedDomainEvent will be dispatched
        // which will trigger the creation of the first round

        return Result.Success();
    }
}
