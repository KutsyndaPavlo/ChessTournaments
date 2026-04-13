using System.Reflection;
using Carter;
using ChessTournaments.Modules.TournamentRequests.API.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.TournamentRequests.API;

public class TournamentRequestsEndpointGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tournament-requests")
            .WithTags("Tournament Requests")
            .RequireAuthorization();

        // Automatically discover and register all endpoint classes that implement IEndpoint
        var endpointTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t)
            );

        foreach (var endpointType in endpointTypes)
        {
            var endpoint = (IEndpoint?)Activator.CreateInstance(endpointType);
            endpoint?.MapEndpoint(group);
        }
    }
}
