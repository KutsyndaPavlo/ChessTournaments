using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.RemoveMatchTag;

public record RemoveMatchTagCommand(Guid MatchId, string TagName) : IRequest<Result>;
