using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CreateTournament;

public class CreateTournamentCommandHandler
    : IRequestHandler<CreateTournamentCommand, Result<TournamentDto>>
{
    private readonly ITournamentRepository _repository;

    public CreateTournamentCommandHandler(ITournamentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TournamentDto>> Handle(
        CreateTournamentCommand request,
        CancellationToken cancellationToken
    )
    {
        var settings = new TournamentSettings(
            request.Format,
            request.TimeControl,
            request.TimeInMinutes,
            request.IncrementInSeconds,
            request.NumberOfRounds,
            request.MaxPlayers,
            request.MinPlayers,
            request.AllowByes,
            request.EntryFee
        );

        var tournamentResult = Tournament.Create(
            request.Name,
            request.Description,
            request.StartDate,
            settings,
            request.OrganizerId,
            request.Location
        );

        if (tournamentResult.IsFailure)
            return Result.Failure<TournamentDto>(tournamentResult.Error);

        await _repository.AddAsync(tournamentResult.Value, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new TournamentDto
        {
            Id = tournamentResult.Value.Id,
            Name = tournamentResult.Value.Name,
            Description = tournamentResult.Value.Description,
            StartDate = tournamentResult.Value.StartDate,
            EndDate = tournamentResult.Value.EndDate,
            Status = tournamentResult.Value.Status,
            OrganizerId = tournamentResult.Value.OrganizerId,
            Location = tournamentResult.Value.Location,
            Settings = new TournamentSettingsDto
            {
                Format = tournamentResult.Value.Settings.Format,
                TimeControl = tournamentResult.Value.Settings.TimeControl,
                TimeInMinutes = tournamentResult.Value.Settings.TimeInMinutes,
                IncrementInSeconds = tournamentResult.Value.Settings.IncrementInSeconds,
                NumberOfRounds = tournamentResult.Value.Settings.NumberOfRounds,
                MaxPlayers = tournamentResult.Value.Settings.MaxPlayers,
                MinPlayers = tournamentResult.Value.Settings.MinPlayers,
                AllowByes = tournamentResult.Value.Settings.AllowByes,
                EntryFee = tournamentResult.Value.Settings.EntryFee,
            },
            PlayerCount = tournamentResult.Value.Players.Count,
            RoundCount = tournamentResult.Value.Rounds.Count,
            CreatedAt = tournamentResult.Value.CreatedAt,
        };

        return Result.Success(dto);
    }
}
