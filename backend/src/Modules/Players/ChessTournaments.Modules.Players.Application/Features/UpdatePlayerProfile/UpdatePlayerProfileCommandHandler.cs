using ChessTournaments.Modules.Players.Application.Abstractions;
using ChessTournaments.Modules.Players.Domain.Players;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Players.Application.Features.UpdatePlayerProfile;

public class UpdatePlayerProfileCommandHandler
    : IRequestHandler<UpdatePlayerProfileCommand, Result<PlayerDto>>
{
    private readonly IPlayerRepository _repository;

    public UpdatePlayerProfileCommandHandler(IPlayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PlayerDto>> Handle(
        UpdatePlayerProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var player = await _repository.GetByIdAsync(request.PlayerId, cancellationToken);
        if (player is null)
            return Result.Failure<PlayerDto>("Player not found");

        var updateResult = player.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Country,
            request.DateOfBirth,
            request.Bio,
            request.AvatarUrl
        );

        if (updateResult.IsFailure)
            return Result.Failure<PlayerDto>(updateResult.Error);

        await _repository.UpdateAsync(player, cancellationToken);
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
