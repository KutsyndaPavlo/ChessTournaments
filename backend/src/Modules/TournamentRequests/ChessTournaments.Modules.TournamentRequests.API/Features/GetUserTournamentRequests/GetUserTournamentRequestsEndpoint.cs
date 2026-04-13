using System.Security.Claims;
using ChessTournaments.Modules.TournamentRequests.API.Common;
using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Application.Features.GetUserTournamentRequests;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.TournamentRequests.API.Features.GetUserTournamentRequests;

public class GetUserTournamentRequestsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/my-requests",
                async Task<
                    Results<
                        Ok<IEnumerable<TournamentRequestDto>>,
                        BadRequest<ErrorResponse>,
                        UnauthorizedHttpResult
                    >
                > (HttpContext context, ISender sender) =>
                {
                    var userId = context.User.FindFirstValue("sub");
                    if (string.IsNullOrEmpty(userId))
                        return TypedResults.Unauthorized();

                    var result = await sender.Send(new GetUserTournamentRequestsQuery(userId));

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok<IEnumerable<TournamentRequestDto>>(result.Value);
                }
            )
            .RequireAuthorization();
    }
}
