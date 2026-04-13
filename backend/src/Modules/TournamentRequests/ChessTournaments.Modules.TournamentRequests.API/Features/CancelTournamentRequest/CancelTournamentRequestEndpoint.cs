using ChessTournaments.Modules.TournamentRequests.API.Common;
using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Application.Features.CancelTournamentRequest;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.TournamentRequests.API.Features.CancelTournamentRequest;

public class CancelTournamentRequestEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/{requestId:guid}/cancel",
                async Task<Results<Ok<TournamentRequestDto>, BadRequest<ErrorResponse>>> (
                    Guid requestId,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(new CancelTournamentRequestCommand(requestId));

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok(result.Value);
                }
            )
            .RequireAuthorization();
    }
}
