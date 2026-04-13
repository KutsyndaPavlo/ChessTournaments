using ChessTournaments.API.Infrastructure.OpenApi.Transformers;
using Scalar.AspNetCore;

namespace ChessTournaments.API.Infrastructure.OpenApi;

public static class OpenApiExtensions
{
    public static void AddCustomizedOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddOpenApi(options =>
        {
            options.CreateSchemaReferenceId = CustomSchemaIdFactory.CreateSchemaReferenceId;

            options
                .AddSchemaTransformer<JsonStringEnumSchemaTransformer>()
                .AddSchemaTransformer<EnsureUniqueIdSchemaTransformer>()
                .AddSchemaTransformer<CustomSchemaIdSchemaTransformer>()
                .AddOperationTransformer<AuthorizedEndpointOperationTransformer>()
                .AddDocumentTransformer<ApiInfoDocumentTransformer>()
                .AddDocumentTransformer<BearerSecuritySchemeDocumentTransformer>();
        });
    }

    public static void MapCustomizedOpenApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(
            "/",
            options =>
            {
                options.WithTitle(OpenApiMetadata.ApiName).EnablePersistentAuthentication();
                options.Favicon = "favicon.ico";
            }
        );
    }
}
