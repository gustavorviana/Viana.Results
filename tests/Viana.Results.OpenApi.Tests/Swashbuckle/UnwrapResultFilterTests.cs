using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Viana.Results.OpenApi.Swashbuckle.Filters;

namespace Viana.Results.OpenApi.Tests.Swashbuckle;

public class UnwrapResultFilterTests
{
    public sealed class Forecast
    {
        public int TemperatureC { get; set; }
    }

    public Result<Forecast> Get() => Results.Ok(new Forecast());

    [Fact]
    public void Apply_ReplacesSchemaButKeepsExamples()
    {
        var api = new ApiDescription();
        api.SupportedResponseTypes.Add(new ApiResponseType { StatusCode = 200, Type = typeof(Result<Forecast>) });

        var example = JsonNode.Parse("""{"temperatureC":22}""");
        var examples = new Dictionary<string, IOpenApiExample> { ["typical"] = new OpenApiExample { Value = example } };
        var media = new OpenApiMediaType { Example = example, Examples = examples };
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = media } }
            }
        };

        var generator = new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new JsonSerializerOptions()));
        var method = typeof(UnwrapResultFilterTests).GetMethod(nameof(Get), BindingFlags.Public | BindingFlags.Instance)!;
        var context = new OperationFilterContext(api, generator, new SchemaRepository(), new OpenApiDocument(), method);

        new UnwrapResultFilter().Apply(operation, context);

        var after = operation.Responses["200"].Content!["application/json"];
        Assert.Same(media, after);
        Assert.Same(example, after.Example);
        Assert.Same(examples, after.Examples);
        Assert.NotNull(after.Schema);
    }
}
