using IgniteLifeApi.Presentation.OpenApi.Attributes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace IgniteLifeApi.Presentation.OpenApi.Transformers;

/// <summary>
/// An OpenAPI operation transformer that restricts media types to application/json
/// for endpoints decorated with the [JsonOnly] attribute.
/// </summary>
public class JsonOnlyOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
    OpenApiOperation operation,
    OpenApiOperationTransformerContext context,
    CancellationToken cancellationToken)
    {
        var hasJsonOnly = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<JsonOnlyAttribute>()
            .Any();

        if (!hasJsonOnly)
            return Task.CompletedTask;

        // Clean request content types
        if (operation.RequestBody?.Content != null &&
            operation.RequestBody.Content.TryGetValue("application/json", out var jsonBody))
        {
            operation.RequestBody.Content.Clear();
            operation.RequestBody.Content["application/json"] = jsonBody;
        }

        // Clean response content types
        foreach (var response in operation.Responses.Values)
        {
            if (response.Content.TryGetValue("application/json", out var jsonResp))
            {
                response.Content.Clear();
                response.Content["application/json"] = jsonResp;
            }
        }

        return Task.CompletedTask;
    }
}
