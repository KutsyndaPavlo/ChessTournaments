using ChessTournaments.Modules.Players.API.Common;
using ChessTournaments.Modules.Players.Application.Abstractions;
using ChessTournaments.Modules.Players.Application.Features.UpdatePlayerProfile;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Players.API.Features.UpdatePlayerProfile;

public class UpdatePlayerProfileEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPut(
                "/{playerId:guid}",
                async Task<Results<Ok<PlayerDto>, BadRequest<ErrorResponse>>> (
                    Guid playerId,
                    UpdatePlayerProfileCommand command,
                    ISender sender
                ) =>
                {
                    // Ensure the playerId from route matches the command
                    var updatedCommand = command with
                    {
                        PlayerId = playerId,
                    };
                    var result = await sender.Send(updatedCommand);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok(result.Value);
                }
            )
            .RequireAuthorization();
    }
}
