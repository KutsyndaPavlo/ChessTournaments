using ChessTournaments.Modules.Players.Application.Abstractions;
using ChessTournaments.Modules.Players.Domain.Players;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Players.Application.Features.CreatePlayer;

public class CreatePlayerCommandHandler : IRequestHandler<CreatePlayerCommand, Result<PlayerDto>>
{
    private readonly IPlayerRepository _repository;

    public CreatePlayerCommandHandler(IPlayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PlayerDto>> Handle(
        CreatePlayerCommand request,
        CancellationToken cancellationToken
    )
    {
        // Check if player already exists
        var existingPlayer = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existingPlayer is not null)
            return Result.Failure<PlayerDto>("Player profile already exists for this user");

        var playerResult = Player.Create(
            request.UserId,
            request.FirstName,
            request.LastName,
            request.InitialRating,
            request.Country,
            request.DateOfBirth
        );

        if (playerResult.IsFailure)
            return Result.Failure<PlayerDto>(playerResult.Error);

        var player = playerResult.Value;

        await _repository.AddAsync(player, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(player));
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
