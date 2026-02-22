using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Viana.Results.FullExample.Swagger;

public class UnwrapResultFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null) return;

        foreach (var response in operation.Responses)
        {
            if (response.Value.Content == null)
                continue;

            var responseType = context.ApiDescription.SupportedResponseTypes
                .FirstOrDefault(x => x.StatusCode.ToString() == response.Key)?.Type;

            if (responseType == null)
                continue;

            if (responseType == typeof(Result))
            {
                response.Value.Content.Clear();
                continue;
            }

            if (!ResultReflections.IsUnwrappableType(responseType))
                continue;

            var genericArgType = responseType.GetGenericArguments()[0];
            if (ResultReflections.IsScalarLike(genericArgType))
                continue;

            var schema = context.SchemaGenerator.GenerateSchema(responseType, context.SchemaRepository);
            var keys = response.Value.Content.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                response.Value.Content[key] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(genericArgType, context.SchemaRepository)
                };
            }
        }
    }
}