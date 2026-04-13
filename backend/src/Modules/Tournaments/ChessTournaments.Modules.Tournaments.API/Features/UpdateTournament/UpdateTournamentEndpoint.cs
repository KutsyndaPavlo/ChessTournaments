using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.UpdateTournament;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.UpdateTournament;

public class UpdateTournamentEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPut(
                "/{id:guid}",
                async Task<Results<Ok<TournamentDto>, BadRequest<ErrorResponse>>> (
                    Guid id,
                    UpdateTournamentRequest request,
                    ISender sender
                ) =>
                {
                    var command = new UpdateTournamentCommand(
                        id,
                        request.Name,
                        request.Description,
                        request.Location
                    );
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok(result.Value);
                }
            )
            .RequireAuthorization("AdminPolicy");
    }
}

public record UpdateTournamentRequest(string Name, string Description, string Location);
