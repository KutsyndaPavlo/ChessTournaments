using ChessTournaments.Modules.Players.API.Common;
using ChessTournaments.Modules.Players.Application.Abstractions;
using ChessTournaments.Modules.Players.Application.Features.GetTopPlayers;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Players.API.Features.GetTopPlayers;

public class GetTopPlayersEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/top",
                async Task<Results<Ok<IEnumerable<PlayerDto>>, BadRequest<ErrorResponse>>> (
                    int count,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(new GetTopPlayersQuery(count));

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok<IEnumerable<PlayerDto>>(result.Value);
                }
            )
            .RequireAuthorization();
    }
}
