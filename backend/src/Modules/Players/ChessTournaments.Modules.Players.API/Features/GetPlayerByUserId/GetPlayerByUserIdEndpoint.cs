using ChessTournaments.Modules.Players.API.Common;
using ChessTournaments.Modules.Players.Application.Abstractions;
using ChessTournaments.Modules.Players.Application.Features.CreatePlayer;
using ChessTournaments.Modules.Players.Application.Features.GetPlayerByUserId;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Players.API.Features.GetPlayerByUserId;

public class GetPlayerByUserIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/user/{userId}",
                async Task<
                    Results<Ok<PlayerDto>, BadRequest<ErrorResponse>, NotFound<ErrorResponse>>
                > (string userId, HttpContext httpContext, ISender sender) =>
                {
                    var result = await sender.Send(new GetPlayerByUserIdQuery(userId));

                    if (result.IsFailure)
                    {
                        // temporary
                        var claims = httpContext.User.Claims;
                        var email = claims.FirstOrDefault(c => c.Type == "email")?.Value;

                        var createPlayerCommand = new CreatePlayerCommand(
                            UserId: userId,
                            FirstName: email,
                            LastName: email
                        );

                        var createResult = await sender.Send(createPlayerCommand);

                        if (createResult.IsFailure)
                            return TypedResults.BadRequest(new ErrorResponse(createResult.Error));

                        result = await sender.Send(new GetPlayerByUserIdQuery(userId));
                    }

                    if (result.IsFailure)
                        return TypedResults.NotFound(new ErrorResponse(result.Error));

                    return TypedResults.Ok(result.Value);
                }
            )
            .RequireAuthorization();
    }
}
