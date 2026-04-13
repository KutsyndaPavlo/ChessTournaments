using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ChessTournaments.API.Infrastructure.OpenApi.Transformers;

internal sealed class JsonStringEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var type = context.JsonTypeInfo.Type;
        var underlyingType = Nullable.GetUnderlyingType(type);

        // Check if it's an enum or a nullable enum
        if (type.IsEnum || underlyingType?.IsEnum == true)
        {
            schema.Type = JsonSchemaType.String;
        }

        // Handle nullable enums - check if last enum value is null
        if (schema.Enum != null && schema.Enum.Count > 0)
        {
            var lastEnum = schema.Enum[schema.Enum.Count - 1];
            if (lastEnum == null)
            {
                schema.Enum.RemoveAt(schema.Enum.Count - 1);
            }
        }

        return Task.CompletedTask;
    }
}
