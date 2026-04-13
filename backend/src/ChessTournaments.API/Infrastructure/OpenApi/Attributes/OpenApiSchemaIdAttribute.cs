namespace ChessTournaments.API.Infrastructure.OpenApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
internal sealed class OpenApiSchemaIdAttribute(string schemaId) : Attribute
{
    public string SchemaId { get; } = schemaId;
}
