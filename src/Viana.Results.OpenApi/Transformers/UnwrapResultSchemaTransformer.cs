using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Viana.Results.OpenApi.Transformers;

/// <summary>
/// OpenAPI schema transformer that unwraps result wrapper types.
/// For <c>Result&lt;T&gt;</c> and <c>ListResult&lt;T&gt;</c>, replaces the wrapper schema content with
/// the inner data schema (handling both inline schemas and <c>$ref</c> references via <c>allOf</c>).
/// <c>PagedResult&lt;T&gt;</c> is preserved because it carries paging metadata.
/// </summary>
public sealed class UnwrapResultSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema?.Properties is null)
            return Task.CompletedTask;

        var type = context.JsonTypeInfo.Type;

        if (!ResultReflections.IsUnwrappableType(type))
            return Task.CompletedTask;

        var dataType = ResultReflections.GetDataType(type);
        if (dataType == null || ResultReflections.IsScalarLike(dataType))
            return Task.CompletedTask;

        if (!schema.Properties.TryGetValue("data", out var dataSchema))
            return Task.CompletedTask;

        // Replace the wrapper schema with the data schema. Using AllOf supports both inline
        // schemas and OpenApiSchemaReference uniformly without losing the $ref linkage.
        schema.Properties = null;
        schema.Required = null;
        schema.Type = null;
        schema.AllOf = new List<IOpenApiSchema> { dataSchema };

        return Task.CompletedTask;
    }
}
