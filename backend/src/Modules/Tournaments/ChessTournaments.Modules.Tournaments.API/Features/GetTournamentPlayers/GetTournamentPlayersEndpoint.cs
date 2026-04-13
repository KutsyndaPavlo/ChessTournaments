using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentPlayers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.GetTournamentPlayers;

public class GetTournamentPlayersEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet(
            "/{id:guid}/players",
            async Task<Ok<IEnumerable<TournamentPlayerDto>>> (Guid id, ISender sender) =>
            {
                var players = await sender.Send(new GetTournamentPlayersQuery(id));
                return TypedResults.Ok(players);
            }
        );
    }
}
