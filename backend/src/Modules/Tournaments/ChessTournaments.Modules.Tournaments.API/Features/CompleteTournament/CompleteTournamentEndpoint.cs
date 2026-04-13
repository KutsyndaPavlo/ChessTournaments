using ChessTournaments.Modules.Tournaments.Application.Features.CompleteTournament;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.CompleteTournament;

public class CompleteTournamentEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/{id:guid}/complete",
                async Task<Results<Ok, BadRequest<ErrorResponse>>> (Guid id, ISender sender) =>
                {
                    var result = await sender.Send(new CompleteTournamentCommand(id));

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok();
                }
            )
            .RequireAuthorization("AdminPolicy");
    }
}
