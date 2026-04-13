using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Matches.API.Common;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
