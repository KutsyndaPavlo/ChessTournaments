//using ChessTournaments.Modules.Tournaments.API.Common;
//using ChessTournaments.Modules.Tournaments.Application.Features.RegisterPlayer;
//using MediatR;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Routing;

//namespace ChessTournaments.Modules.Tournaments.API.Features.RegisterPlayer;

//public class RegisterPlayerEndpoint : IEndpoint
//{
//    public void MapEndpoint(RouteGroupBuilder group)
//    {
//        group.MapPost(
//            "/{id:guid}/register-player",
//            async (Guid id, RegisterPlayerRequest request, ISender sender) =>
//            {
//                var result = await sender.Send(
//                    new RegisterPlayerCommand(
//                        id,
//                        request.PlayerId,
//                        request.PlayerName,
//                        request.Rating
//                    )
//                );

//                if (result.IsFailure)
//                    return Results.BadRequest(new ErrorResponse(result.Error));

//                return Results.Ok();
//            }
//        );
//    }
//}

//public record RegisterPlayerRequest(string PlayerId, string PlayerName, int? Rating);
