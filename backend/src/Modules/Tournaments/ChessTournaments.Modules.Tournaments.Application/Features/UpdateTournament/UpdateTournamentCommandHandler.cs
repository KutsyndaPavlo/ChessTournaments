using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Domain.Common;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.UpdateTournament;

public class UpdateTournamentCommandHandler
    : IRequestHandler<UpdateTournamentCommand, Result<TournamentDto>>
{
    private readonly ITournamentRepository _repository;

    public UpdateTournamentCommandHandler(ITournamentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TournamentDto>> Handle(
        UpdateTournamentCommand request,
        CancellationToken cancellationToken
    )
    {
        var tournament = await _repository.GetByIdAsync(request.TournamentId, cancellationToken);

        if (tournament == null)
            return Result.Failure<TournamentDto>(DomainErrors.Tournament.NotFound.Message);

        var result = tournament.UpdateDetails(request.Name, request.Description, request.Location);

        if (result.IsFailure)
            return Result.Failure<TournamentDto>(result.Error);

        await _repository.UpdateAsync(tournament, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new TournamentDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            Description = tournament.Description,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            Status = tournament.Status,
            OrganizerId = tournament.OrganizerId,
            Location = tournament.Location,
            Settings = new TournamentSettingsDto
            {
                Format = tournament.Settings.Format,
                TimeControl = tournament.Settings.TimeControl,
                TimeInMinutes = tournament.Settings.TimeInMinutes,
                IncrementInSeconds = tournament.Settings.IncrementInSeconds,
                NumberOfRounds = tournament.Settings.NumberOfRounds,
                MaxPlayers = tournament.Settings.MaxPlayers,
                MinPlayers = tournament.Settings.MinPlayers,
                AllowByes = tournament.Settings.AllowByes,
                EntryFee = tournament.Settings.EntryFee,
            },
            PlayerCount = tournament.Players.Count,
            RoundCount = tournament.Rounds.Count,
            CreatedAt = tournament.CreatedAt,
        };

        return Result.Success(dto);
    }
}
