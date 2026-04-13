using ChessTournaments.Modules.TournamentRequests.API.Common;
using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Application.Features.GetPendingTournamentRequests;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.TournamentRequests.API.Features.GetPendingTournamentRequests;

public class GetPendingTournamentRequestsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/pending",
                async Task<
                    Results<Ok<IEnumerable<TournamentRequestDto>>, BadRequest<ErrorResponse>>
                > (ISender sender) =>
                {
                    var result = await sender.Send(new GetPendingTournamentRequestsQuery());

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok<IEnumerable<TournamentRequestDto>>(result.Value);
                }
            )
            .RequireAuthorization("AdminPolicy");
    }
}
