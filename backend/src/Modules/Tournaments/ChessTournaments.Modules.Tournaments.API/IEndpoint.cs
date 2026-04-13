using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Tournaments.API;

/// <summary>
/// Marker interface for endpoint classes that can be automatically discovered and registered.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the provided route group.
    /// </summary>
    /// <param name="group">The route group builder to map the endpoint to.</param>
    void MapEndpoint(RouteGroupBuilder group);
}
