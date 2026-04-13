using ChessTournaments.Modules.Matches.API.Common;
using ChessTournaments.Modules.Matches.Application.Features.RecordMatchResult;
using ChessTournaments.Shared.Domain.Enums;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Matches.API.Features.RecordMatchResult;

public record RecordMatchResultRequest(GameResult Result, string? Moves = null);

public class RecordMatchResultEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/matches/{matchId:guid}/result",
                async Task<Results<NoContent, BadRequest<ErrorResponse>>> (
                    Guid matchId,
                    RecordMatchResultRequest request,
                    ISender sender
                ) =>
                {
                    var command = new RecordMatchResultCommand(
                        matchId,
                        request.Result,
                        request.Moves
                    );
                    var result = await sender.Send(command);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.NoContent();
                }
            )
            .WithTags("Matches")
            .WithName("RecordMatchResult");
    }
}
