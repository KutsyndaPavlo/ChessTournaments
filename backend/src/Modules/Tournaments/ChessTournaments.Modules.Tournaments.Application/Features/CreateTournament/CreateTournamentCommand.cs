using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Shared.Domain.Enums;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CreateTournament;

public record CreateTournamentCommand(
    string Name,
    string Description,
    DateTime StartDate,
    string Location,
    string OrganizerId,
    TournamentFormat Format,
    TimeControl TimeControl,
    int TimeInMinutes,
    int IncrementInSeconds,
    int NumberOfRounds,
    int MaxPlayers,
    int MinPlayers,
    bool AllowByes,
    decimal EntryFee
) : IRequest<Result<TournamentDto>>;
