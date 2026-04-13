using ChessTournaments.Modules.Players.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Players.Application.Features.GetPlayerByUserId;

public record GetPlayerByUserIdQuery(string UserId) : IRequest<Result<PlayerDto>>;
