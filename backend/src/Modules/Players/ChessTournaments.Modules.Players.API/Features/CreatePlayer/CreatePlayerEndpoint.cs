using ChessTournaments.Modules.Players.API.Common;
using ChessTournaments.Modules.Players.Application.Abstractions;
using ChessTournaments.Modules.Players.Application.Features.CreatePlayer;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Players.API.Features.CreatePlayer;

public class CreatePlayerEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/",
                async Task<Results<Created<PlayerDto>, BadRequest<ErrorResponse>>> (
                    CreatePlayerCommand command,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Created($"/api/players/{result.Value.Id}", result.Value);
                }
            )
            .RequireAuthorization();
    }
}
