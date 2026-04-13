using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentRounds;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.GetTournamentRounds;

public class GetTournamentRoundsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet(
            "/{id:guid}/rounds",
            async Task<Ok<IEnumerable<RoundDto>>> (Guid id, ISender sender) =>
            {
                var rounds = await sender.Send(new GetTournamentRoundsQuery(id));
                return TypedResults.Ok(rounds);
            }
        );
    }
}
