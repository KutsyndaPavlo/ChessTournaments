using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.CreateTournament;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.CreateTournament;

public class CreateTournamentEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/",
                async Task<Results<Created<TournamentDto>, BadRequest<ErrorResponse>>> (
                    CreateTournamentCommand command,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Created(
                        $"/api/tournaments/{result.Value.Id}",
                        result.Value
                    );
                }
            )
            .RequireAuthorization("AdminPolicy");
    }
}
