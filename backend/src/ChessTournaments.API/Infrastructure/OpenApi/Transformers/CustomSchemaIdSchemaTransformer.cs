using System.Reflection;
using ChessTournaments.API.Infrastructure.OpenApi.Attributes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ChessTournaments.API.Infrastructure.OpenApi.Transformers;

internal sealed class CustomSchemaIdSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var type = context.JsonTypeInfo.Type;

        var attribute = type.GetCustomAttribute<OpenApiSchemaIdAttribute>();
        if (attribute is null)
        {
            return Task.CompletedTask;
        }

        // Set the schema ID used by Scalar/Swagger UI
        schema.Metadata ??= new Dictionary<string, object>();
        schema.Metadata["x-schema-id"] = attribute.SchemaId;

        return Task.CompletedTask;
    }
}
