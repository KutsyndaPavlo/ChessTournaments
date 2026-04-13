using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.AddMatchTag;

public record AddMatchTagCommand(Guid MatchId, string TagName) : IRequest<Result>;
