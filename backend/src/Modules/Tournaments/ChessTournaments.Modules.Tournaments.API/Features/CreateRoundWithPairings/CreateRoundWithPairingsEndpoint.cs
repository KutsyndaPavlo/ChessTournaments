using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.CreateRoundWithPairings;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.CreateRoundWithPairings;

public class CreateRoundWithPairingsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/{tournamentId:guid}/rounds",
                async Task<Results<Created<RoundDto>, BadRequest<ErrorResponse>>> (
                    Guid tournamentId,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(
                        new CreateRoundWithPairingsCommand(tournamentId)
                    );

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Created(
                        $"/api/tournaments/{tournamentId}/rounds/{result.Value.Id}",
                        result.Value
                    );
                }
            )
            .RequireAuthorization("AdminPolicy");
    }
}
