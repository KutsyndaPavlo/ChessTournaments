using ChessTournaments.Modules.Players.Application.Abstractions;
using ChessTournaments.Modules.Players.Domain.Players;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Players.Application.Features.GetTopPlayers;

public class GetTopPlayersQueryHandler
    : IRequestHandler<GetTopPlayersQuery, Result<List<PlayerDto>>>
{
    private readonly IPlayerRepository _repository;

    public GetTopPlayersQueryHandler(IPlayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<PlayerDto>>> Handle(
        GetTopPlayersQuery request,
        CancellationToken cancellationToken
    )
    {
        var players = await _repository.GetTopByRatingAsync(request.Count, cancellationToken);
        var dtos = players.Select(MapToDto).ToList();
        return Result.Success(dtos);
    }

    private static PlayerDto MapToDto(Player player) =>
        new()
        {
            Id = player.Id,
            UserId = player.UserId,
            FirstName = player.FirstName,
            LastName = player.LastName,
            FullName = player.FullName,
            Country = player.Country,
            DateOfBirth = player.DateOfBirth,
            Bio = player.Bio,
            AvatarUrl = player.AvatarUrl,
            Rating = player.Rating,
            PeakRating = player.PeakRating,
            PeakRatingDate = player.PeakRatingDate,
            TotalGamesPlayed = player.TotalGamesPlayed,
            Wins = player.Wins,
            Losses = player.Losses,
            Draws = player.Draws,
            TournamentsParticipated = player.TournamentsParticipated,
            TournamentsWon = player.TournamentsWon,
            WinRate = player.WinRate,
            DrawRate = player.DrawRate,
            LossRate = player.LossRate,
            CreatedAt = player.CreatedAt,
            UpdatedAt = player.UpdatedAt,
        };
}
