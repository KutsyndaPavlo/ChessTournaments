using ChessTournaments.Modules.Matches.API.Common;
using ChessTournaments.Modules.Matches.Application.Features.RemoveMatchTag;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Matches.API.Features.RemoveMatchTag;

public class RemoveMatchTagEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/matches/{matchId:guid}/tags/{tagName}",
                async Task<Results<NoContent, BadRequest<ErrorResponse>>> (
                    Guid matchId,
                    string tagName,
                    ISender sender
                ) =>
                {
                    var command = new RemoveMatchTagCommand(matchId, tagName);
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.NoContent();
                }
            )
            .WithTags("Matches")
            .WithName("RemoveMatchTag");
    }
}
