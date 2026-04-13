using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Players.API.Common;

public interface IEndpoint
{
    void MapEndpoint(RouteGroupBuilder group);
}
