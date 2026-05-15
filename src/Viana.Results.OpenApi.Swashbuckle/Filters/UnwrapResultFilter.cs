using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Viana.Results.OpenApi.Swashbuckle.Filters;

/// <summary>
/// Swagger operation filter that unwraps result wrapper types from the generated OpenAPI response.
/// For <c>Result&lt;T&gt;</c> and <c>ListResult&lt;T&gt;</c>, replaces the response media schema with
/// the schema of the inner payload type (<c>T</c> or <c>IReadOnlyList&lt;T&gt;</c> respectively).
/// <c>PagedResult&lt;T&gt;</c> is preserved because it carries paging metadata.
/// </summary>
public class UnwrapResultFilter : IOperationFilter
{
    /// <summary>
    /// Applies the filter to the specified OpenAPI operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation being processed.</param>
    /// <param name="context">The operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null) return;

        foreach (var response in operation.Responses)
        {
            if (response.Value.Content == null)
                continue;

            var responseType = context.ApiDescription.SupportedResponseTypes
                .FirstOrDefault(x => x.StatusCode.ToString() == response.Key)?.Type;

            if (responseType == null || !ResultReflections.IsUnwrappableType(responseType))
                continue;

            var dataType = ResultReflections.GetDataType(responseType);
            if (dataType == null || ResultReflections.IsScalarLike(dataType))
                continue;

            var keys = response.Value.Content.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                response.Value.Content[key] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(dataType, context.SchemaRepository)
                };
            }
        }
    }
}
