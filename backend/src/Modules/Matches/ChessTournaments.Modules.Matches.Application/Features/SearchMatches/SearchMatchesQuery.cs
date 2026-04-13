using ChessTournaments.Modules.Matches.Application.Abstractions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.SearchMatches;

public record SearchMatchesQuery(
    string? PlayerId = null,
    string[]? Tags = null,
    Guid? TournamentId = null,
    Guid? RoundId = null
) : IRequest<IEnumerable<TournamentMatchDto>>;
