using ChessTournaments.Modules.Players.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Players.Application.Features.UpdatePlayerProfile;

public record UpdatePlayerProfileCommand(
    Guid PlayerId,
    string FirstName,
    string LastName,
    string? Country = null,
    DateTime? DateOfBirth = null,
    string? Bio = null,
    string? AvatarUrl = null
) : IRequest<Result<PlayerDto>>;
