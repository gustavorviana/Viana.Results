using System.Collections.Generic;
using Microsoft.OpenApi;
using Viana.Results.OpenApi.Schemas;

namespace Viana.Results.OpenApi.Processing;

internal static class DocumentProcessor
{
    internal const string ProblemComponentName = "VianaProblemDetails";

    internal static void RemoveBareResultSchema(OpenApiDocument document)
    {
        document.Components?.Schemas?.Remove("Result");
    }

    // FR-017: the base problem schema is emitted once as a component; responses reference it, and the
    // ones with extra members use allOf [base, own members].
    internal static void ApplySharedProblemSchema(OpenApiDocument document)
    {
        if (document.Paths == null)
            return;

        var used = false;
        foreach (var path in document.Paths.Values)
        {
            if (path.Operations == null)
                continue;
            foreach (var operation in path.Operations.Values)
            {
                if (operation.Responses == null)
                    continue;
                foreach (var response in operation.Responses.Values)
                {
                    if (response.Content == null)
                        continue;
                    foreach (var media in response.Content.Values)
                    {
                        if (!ProblemSchemaRegistry.TryGetExtras(media.Schema, out var extras))
                            continue;

                        var reference = new OpenApiSchemaReference(ProblemComponentName, document);
                        media.Schema = extras.Count == 0
                            ? reference
                            : new OpenApiSchema
                            {
                                AllOf = new List<IOpenApiSchema>
                                {
                                    reference,
                                    new OpenApiSchema { Type = JsonSchemaType.Object, Properties = new Dictionary<string, IOpenApiSchema>(extras) }
                                }
                            };
                        used = true;
                    }
                }
            }
        }

        if (!used)
            return;

        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        document.Components.Schemas[ProblemComponentName] = ProblemSchemaRegistry.CreateBaseSchema();
    }
}
