using ChessTournaments.Modules.Tournaments.Domain.Common;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.RegisterPlayer;

public class RegisterPlayerCommandHandler : IRequestHandler<RegisterPlayerCommand, Result>
{
    private readonly ITournamentRepository _repository;

    public RegisterPlayerCommandHandler(ITournamentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        RegisterPlayerCommand request,
        CancellationToken cancellationToken
    )
    {
        var tournament = await _repository.GetByIdAsync(request.TournamentId, cancellationToken);

        if (tournament == null)
            return Result.Failure(DomainErrors.Tournament.NotFound.Message);

        var result = tournament.RegisterPlayer(
            request.PlayerId,
            request.PlayerName,
            request.Rating
        );

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _repository.UpdateAsync(tournament, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
