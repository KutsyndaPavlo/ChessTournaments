using ChessTournaments.Modules.Matches.API.Common;
using ChessTournaments.Modules.Matches.Application.Abstractions;
using ChessTournaments.Modules.Matches.Application.Features.SearchMatches;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Matches.API.Features.SearchMatches;

public class SearchMatchesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/matches/search",
                async Task<Ok<IEnumerable<TournamentMatchDto>>> (
                    string? playerId,
                    string? tags,
                    Guid? tournamentId,
                    Guid? roundId,
                    ISender sender
                ) =>
                {
                    var tagArray = !string.IsNullOrWhiteSpace(tags)
                        ? tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        : null;

                    var query = new SearchMatchesQuery(playerId, tagArray, tournamentId, roundId);
                    var result = await sender.Send(query);

                    return TypedResults.Ok(result);
                }
            )
            .WithTags("Matches")
            .WithName("SearchMatches")
            .AllowAnonymous();
    }
}
