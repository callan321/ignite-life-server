using IgniteLifeApi.Presentation.OpenApi.Attributes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace IgniteLifeApi.Presentation.OpenApi.Transformers;

public class AuthCookieOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        bool requiresCookies = metadata.OfType<RequiresAuthCookiesAttribute>().Any();
        bool producesCookies = metadata.OfType<ProducesAuthCookiesAttribute>().Any();

        // Add security requirement so codegen sends credentials with cookies
        if (requiresCookies)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "cookieAuth"
                            }
                        },
                        Array.Empty<string>() // No scopes
                    }
                }
            };

            // Optional: Explicitly show cookies in request params (for Swagger UI)
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "access_token",
                In = ParameterLocation.Cookie,
                Required = false,
                Description = "Access token cookie used for authentication",
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "refresh_token",
                In = ParameterLocation.Cookie,
                Required = false,
                Description = "Refresh token cookie used to re-issue access",
                Schema = new OpenApiSchema { Type = "string" }
            });
        }

        // Add Set-Cookie header if cookies are being written or cleared
        if (producesCookies)
        {
            foreach (var response in operation.Responses.Values)
            {
                response.Headers ??= new Dictionary<string, OpenApiHeader>();
                response.Headers["Set-Cookie"] = new OpenApiHeader
                {
                    Description = "Sets or clears authentication cookies (access_token, refresh_token)",
                    Schema = new OpenApiSchema { Type = "string" }
                };
            }
        }

        return Task.CompletedTask;
    }
}
