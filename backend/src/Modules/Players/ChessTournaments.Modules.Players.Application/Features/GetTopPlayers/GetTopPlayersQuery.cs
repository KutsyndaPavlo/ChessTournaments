using ChessTournaments.Modules.Players.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Players.Application.Features.GetTopPlayers;

public record GetTopPlayersQuery(int Count = 10) : IRequest<Result<List<PlayerDto>>>;
