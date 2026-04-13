using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.GetAllTournaments;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.GetAllTournaments;

public class GetAllTournamentsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            async Task<Ok<IEnumerable<TournamentDto>>> (ISender sender) =>
            {
                var tournaments = await sender.Send(new GetAllTournamentsQuery());
                return TypedResults.Ok(tournaments);
            }
        );
    }
}
