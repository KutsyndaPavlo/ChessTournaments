using ChessTournaments.Modules.Tournaments.Application.Features.CompleteRound;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.CompleteRound;

public class CompleteRoundEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/{tournamentId:guid}/rounds/{roundId:guid}/complete",
                async Task<Results<Ok, BadRequest<ErrorResponse>>> (
                    Guid tournamentId,
                    Guid roundId,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(new CompleteRoundCommand(tournamentId, roundId));

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok();
                }
            )
            .RequireAuthorization("AdminPolicy");
    }
}
