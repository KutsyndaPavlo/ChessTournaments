using ChessTournaments.Modules.Players.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Players.Application.Features.CreatePlayer;

public record CreatePlayerCommand(
    string UserId,
    string FirstName,
    string LastName,
    int InitialRating = 1200,
    string? Country = null,
    DateTime? DateOfBirth = null
) : IRequest<Result<PlayerDto>>;
