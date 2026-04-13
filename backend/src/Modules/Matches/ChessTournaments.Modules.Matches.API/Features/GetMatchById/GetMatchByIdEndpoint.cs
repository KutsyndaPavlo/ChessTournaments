using ChessTournaments.Modules.Matches.API.Common;
using ChessTournaments.Modules.Matches.Application.Abstractions;
using ChessTournaments.Modules.Matches.Application.Features.GetMatchById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Matches.API.Features.GetMatchById;

public class GetMatchByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/matches/{matchId:guid}",
                async Task<Results<Ok<TournamentMatchDto>, NotFound>> (
                    Guid matchId,
                    ISender sender
                ) =>
                {
                    var query = new GetMatchByIdQuery(matchId);
                    var result = await sender.Send(query);

                    if (result is null)
                        return TypedResults.NotFound();

                    return TypedResults.Ok(result);
                }
            )
            .WithTags("Matches")
            .WithName("GetMatchById")
            .AllowAnonymous();
    }
}
