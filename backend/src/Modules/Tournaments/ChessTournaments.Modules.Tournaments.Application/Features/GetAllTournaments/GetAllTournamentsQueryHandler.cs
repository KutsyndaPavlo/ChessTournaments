using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.GetAllTournaments;

public class GetAllTournamentsQueryHandler
    : IRequestHandler<GetAllTournamentsQuery, IEnumerable<TournamentDto>>
{
    private readonly ITournamentRepository _repository;

    public GetAllTournamentsQueryHandler(ITournamentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TournamentDto>> Handle(
        GetAllTournamentsQuery request,
        CancellationToken cancellationToken
    )
    {
        var tournaments = await _repository.GetAllAsync(cancellationToken);

        return tournaments.Select(t => new TournamentDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            Status = t.Status,
            OrganizerId = t.OrganizerId,
            Location = t.Location,
            Settings = new TournamentSettingsDto
            {
                Format = t.Settings.Format,
                TimeControl = t.Settings.TimeControl,
                TimeInMinutes = t.Settings.TimeInMinutes,
                IncrementInSeconds = t.Settings.IncrementInSeconds,
                NumberOfRounds = t.Settings.NumberOfRounds,
                MaxPlayers = t.Settings.MaxPlayers,
                MinPlayers = t.Settings.MinPlayers,
                AllowByes = t.Settings.AllowByes,
                EntryFee = t.Settings.EntryFee,
            },
            PlayerCount = t.Players.Count,
            RoundCount = t.Rounds.Count,
            CreatedAt = t.CreatedAt,
        });
    }
}
