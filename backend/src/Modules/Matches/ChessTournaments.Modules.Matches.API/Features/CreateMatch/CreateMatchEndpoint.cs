using ChessTournaments.Modules.Matches.API.Common;
using ChessTournaments.Modules.Matches.Application.Features.CreateMatch;
using ChessTournaments.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Matches.API.Features.CreateMatch;

public record CreateMatchRequest(
    Guid RoundId,
    Guid TournamentId,
    string WhitePlayerId,
    string BlackPlayerId,
    int BoardNumber
);

public record CreateMatchResponse(Guid Id);

public class CreateMatchEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/matches",
                async Task<Results<Created<CreateMatchResponse>, BadRequest<ErrorResponse>>> (
                    CreateMatchRequest request,
                    ISender sender
                ) =>
                {
                    var command = new CreateMatchCommand(
                        request.RoundId,
                        request.TournamentId,
                        request.WhitePlayerId,
                        request.BlackPlayerId,
                        request.BoardNumber
                    );

                    var result = await sender.Send(command);

                    if (result.IsFailure)
                        return TypedResults.BadRequest(new ErrorResponse(result.Error));

                    return TypedResults.Created(
                        $"/api/matches/{result.Value}",
                        new CreateMatchResponse(result.Value)
                    );
                }
            )
            .WithTags("Matches")
            .WithName("CreateMatch");
    }
}
