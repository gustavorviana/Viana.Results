using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Viana.Results.OpenApi.Swashbuckle.Filters;

namespace Viana.Results.OpenApi.Swashbuckle;

/// <summary>
/// Extension methods to register Viana.Results OpenAPI/Swagger filters in one call.
/// </summary>
public static class SwaggerGenExtensions
{
    public static SwaggerGenOptions AddVianaResultFilters(
        this SwaggerGenOptions options)
    {
        options.SchemaFilter<OmitProblemSchemaFilter>();
        options.OperationFilter<NoResponseBodyOperationFilter>();
        options.DocumentFilter<RemoveResultSchemaDocumentFilter>();
        options.OperationFilter<ProblemResponseOperationFilter>();
        options.OperationFilter<UnwrapResultFilter>();
        return options;
    }
}
