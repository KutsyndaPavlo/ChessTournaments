using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ChessTournaments.API.Infrastructure.OpenApi.Transformers;

internal sealed class AuthorizedEndpointOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        // Check if endpoint has authorization metadata
        var authorizeData = context
            .Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>()
            .ToList();

        var allowAnonymous = context
            .Description.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>()
            .Any();

        // Skip if no authorization required or explicitly allows anonymous
        if (authorizeData.Count == 0 || allowAnonymous)
            return Task.CompletedTask;

        operation.Security ??= [];
        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = [],
            }
        );

        operation.Responses ??= [];
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        return Task.CompletedTask;
    }
}
