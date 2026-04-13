using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.TournamentRequests.API.Common;

public interface IEndpoint
{
    void MapEndpoint(RouteGroupBuilder group);
}
