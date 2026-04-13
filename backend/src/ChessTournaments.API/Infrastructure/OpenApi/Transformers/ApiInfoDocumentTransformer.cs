using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ChessTournaments.API.Infrastructure.OpenApi.Transformers;

internal sealed class ApiInfoDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        document.Info = new()
        {
            Title = OpenApiMetadata.ApiName,
            Version = OpenApiMetadata.ApiVersion,
        };

        return Task.CompletedTask;
    }
}
