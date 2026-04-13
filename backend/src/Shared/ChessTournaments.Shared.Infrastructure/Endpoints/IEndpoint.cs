using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Shared.Infrastructure.Endpoints;

/// <summary>
/// Defines a contract for API endpoint registration.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the application's route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    void MapEndpoint(IEndpointRouteBuilder app);
}
