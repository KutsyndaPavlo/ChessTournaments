using System.Collections.Concurrent;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ChessTournaments.API.Infrastructure.OpenApi.Transformers;

internal sealed class EnsureUniqueIdSchemaTransformer : IOpenApiSchemaTransformer
{
    private readonly ConcurrentDictionary<string, Type> _registeredSchemas = new();

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var type = context.JsonTypeInfo.Type;

        // Microsoft OpenAPI by default doesn't generate any IDs and instead uses the metadata
        var schemaId = schema.Id ?? (schema.Metadata?["x-schema-id"] as string);

        // It is normal for schema IDs to be null or empty.
        // For example, it indicates primitive types like integers and strings, which we don't need to process here
        if (string.IsNullOrEmpty(schemaId))
        {
            return Task.CompletedTask;
        }

        if (!_registeredSchemas.TryAdd(schemaId, context.JsonTypeInfo.Type))
        {
            var existingType = _registeredSchemas[schemaId];
            var existingUnderlying = Nullable.GetUnderlyingType(existingType) ?? existingType;
            var currentUnderlying = Nullable.GetUnderlyingType(type) ?? type;

            if (existingUnderlying != currentUnderlying)
            {
                throw new InvalidOperationException(
                    $"Duplicate schema IDs detected: both {existingType.FullName} and {type.FullName} have the same schema ID {schemaId}"
                );
            }
        }

        return Task.CompletedTask;
    }
}
