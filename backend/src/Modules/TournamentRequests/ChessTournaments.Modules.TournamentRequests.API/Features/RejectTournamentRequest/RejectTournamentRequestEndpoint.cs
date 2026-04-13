using System.Security.Claims;
using ChessTournaments.Modules.TournamentRequests.API.Common;
using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Application.Features.RejectTournamentRequest;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.TournamentRequests.API.Features.RejectTournamentRequest;

public class RejectTournamentRequestEndpoint : IEndpoint
{
    public record RejectRequest(string RejectionReason);

    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/{requestId:guid}/reject",
                async Task<
                    Results<
                        Ok<TournamentRequestDto>,
                        BadRequest<ErrorResponse>,
                        UnauthorizedHttpResult
                    >
                > (Guid requestId, RejectRequest request, HttpContext context, ISender sender) =>
                {
                    var adminId = context.User.FindFirstValue("sub");
                    if (string.IsNullOrEmpty(adminId))
                        return TypedResults.Unauthorized();

                    var result = await sender.Send(
                        new RejectTournamentRequestCommand(
                            requestId,
                            adminId,
                            request.RejectionReason
                        )
                    );

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Ok(result.Value);
                }
            )
            .RequireAuthorization("AdminPolicy");
    }
}
