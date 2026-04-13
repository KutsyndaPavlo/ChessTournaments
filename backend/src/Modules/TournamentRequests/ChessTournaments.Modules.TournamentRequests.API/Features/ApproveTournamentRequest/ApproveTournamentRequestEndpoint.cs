using System.Security.Claims;
using ChessTournaments.Modules.TournamentRequests.API.Common;
using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Application.Features.ApproveTournamentRequest;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.TournamentRequests.API.Features.ApproveTournamentRequest;

public class ApproveTournamentRequestEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/{requestId:guid}/approve",
                async Task<
                    Results<
                        Ok<TournamentRequestDto>,
                        BadRequest<ErrorResponse>,
                        UnauthorizedHttpResult
                    >
                > (Guid requestId, HttpContext context, ISender sender) =>
                {
                    var adminId = context.User.FindFirstValue("sub");
                    if (string.IsNullOrEmpty(adminId))
                        return TypedResults.Unauthorized();

                    var result = await sender.Send(
                        new ApproveTournamentRequestCommand(requestId, adminId)
                    );

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok(result.Value);
                }
            )
            .RequireAuthorization("AdminPolicy");
    }
}
