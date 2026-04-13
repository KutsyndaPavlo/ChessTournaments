using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentById;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.GetTournamentById;

public class GetTournamentByIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet(
            "/{id:guid}",
            async Task<Results<Ok<TournamentDto>, NotFound<ErrorResponse>>> (
                Guid id,
                ISender sender
            ) =>
            {
                var result = await sender.Send(new GetTournamentByIdQuery(id));

                if (result.IsFailure)
                    return TypedResults.NotFound(new ErrorResponse(result.Error));

                return TypedResults.Ok(result.Value);
            }
        );
    }
}
