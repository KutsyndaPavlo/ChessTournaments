using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentsByOrganizer;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API.Features.GetTournamentsByOrganizer;

public class GetTournamentsByOrganizerEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet(
            "/organizer/{organizerId}",
            async Task<Ok<IEnumerable<TournamentDto>>> (string organizerId, ISender sender) =>
            {
                var tournaments = await sender.Send(
                    new GetTournamentsByOrganizerQuery(organizerId)
                );
                return TypedResults.Ok(tournaments);
            }
        );
    }
}
