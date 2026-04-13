using ChessTournaments.Modules.Players.API.Common;
using ChessTournaments.Modules.Players.Domain.Achievements;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ChessTournaments.Modules.Players.API.Features.GetAchievements;

public class GetAchievementsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/{playerId:guid}/achievements",
                async Task<Ok<List<AchievementResponse>>> (
                    Guid playerId,
                    IAchievementRepository repository
                ) =>
                {
                    var achievements = await repository.GetByPlayerIdAsync(playerId);

                    var response = achievements
                        .Select(a => new AchievementResponse(
                            a.Id,
                            a.TournamentId,
                            a.TournamentName,
                            a.Position,
                            a.Score,
                            a.AchievedAt,
                            a.GetMedalEmoji(),
                            a.GetPositionText()
                        ))
                        .ToList();

                    return TypedResults.Ok(response);
                }
            )
            .WithName("GetPlayerAchievements")
            .AllowAnonymous();
    }
}

public record AchievementResponse(
    Guid Id,
    Guid TournamentId,
    string TournamentName,
    int Position,
    decimal Score,
    DateTime AchievedAt,
    string MedalEmoji,
    string PositionText
);
