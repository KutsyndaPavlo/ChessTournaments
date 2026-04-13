using ChessTournaments.Modules.TournamentRequests.API.Common;
using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Application.Features.GetAllTournamentRequests;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.TournamentRequests.API.Features.GetAllTournamentRequests;

public class GetAllTournamentRequestsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/",
                async Task<
                    Results<Ok<IEnumerable<TournamentRequestDto>>, BadRequest<ErrorResponse>>
                > (ISender sender) =>
                {
                    var result = await sender.Send(new GetAllTournamentRequestsQuery());

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok<IEnumerable<TournamentRequestDto>>(result.Value);
                }
            )
            .RequireAuthorization("AdminPolicy");
    }
}
