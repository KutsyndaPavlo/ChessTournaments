using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;

namespace ChessTournaments.API.Infrastructure.OpenApi;

internal static class CustomSchemaIdFactory
{
    public static string? CreateSchemaReferenceId(JsonTypeInfo jsonTypeInfo)
    {
        return jsonTypeInfo.Type.IsNested
            ? CreateSchemaReferenceId(
                JsonTypeInfo.CreateJsonTypeInfo(
                    jsonTypeInfo.Type.DeclaringType!,
                    jsonTypeInfo.Options
                )
            ) + CreateDefaultSchemaReferenceId(jsonTypeInfo)
            : CreateDefaultSchemaReferenceId(jsonTypeInfo);
    }

    private static string? CreateDefaultSchemaReferenceId(JsonTypeInfo jsonTypeInfo) =>
        OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
}
