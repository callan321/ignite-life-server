using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace IgniteLifeApi.Presentation.OpenApi.Transformers;

public class CookieAuthDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        // Register the cookieAuth scheme for authentication
        document.Components.SecuritySchemes["cookieAuth"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Cookie,
            Name = "access_token",
            Description = "Authentication cookie used to authorize requests"
        };

        return Task.CompletedTask;
    }
}
