using ChessTournaments.Modules.Matches.API.Common;
using ChessTournaments.Modules.Matches.Application.Features.AddMatchTag;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Matches.API.Features.AddMatchTag;

public record AddMatchTagRequest(string TagName);

public class AddMatchTagEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/matches/{matchId:guid}/tags",
                async Task<Results<NoContent, BadRequest<ErrorResponse>>> (
                    Guid matchId,
                    AddMatchTagRequest request,
                    ISender sender
                ) =>
                {
                    var command = new AddMatchTagCommand(matchId, request.TagName);
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.NoContent();
                }
            )
            .WithTags("Matches")
            .WithName("AddMatchTag");
    }
}
