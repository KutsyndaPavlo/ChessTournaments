using ChessTournaments.Modules.Matches.API.Common;
using ChessTournaments.Modules.Matches.Application.Features.UpdateMatchMoves;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Matches.API.Features.UpdateMatchMoves;

public record UpdateMatchMovesRequest(string Moves);

public class UpdateMatchMovesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/matches/{matchId:guid}/moves",
                async Task<Results<NoContent, BadRequest<ErrorResponse>>> (
                    Guid matchId,
                    UpdateMatchMovesRequest request,
                    ISender sender
                ) =>
                {
                    var command = new UpdateMatchMovesCommand(matchId, request.Moves);
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.NoContent();
                }
            )
            .WithTags("Matches")
            .WithName("UpdateMatchMoves");
    }
}
