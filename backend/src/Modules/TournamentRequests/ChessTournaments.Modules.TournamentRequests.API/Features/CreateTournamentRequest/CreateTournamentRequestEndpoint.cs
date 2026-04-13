using ChessTournaments.Modules.TournamentRequests.API.Common;
using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Application.Features.CreateTournamentRequest;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.TournamentRequests.API.Features.CreateTournamentRequest;

public class CreateTournamentRequestEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/",
                async Task<Results<Created<TournamentRequestDto>, BadRequest<ErrorResponse>>> (
                    CreateTournamentRequestCommand command,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Created(
                        $"/api/tournament-requests/{result.Value.Id}",
                        result.Value
                    );
                }
            )
            .RequireAuthorization();
    }
}
