using System.Text.Json;

namespace Viana.Results.Examples.Tests;

/// <summary>Every status an operation returns at runtime is documented, with a schema or example, in both documents.</summary>
public class OpenApiCoverageTests
{
    private static readonly (string Path, string Method, string[] Statuses)[] Operations =
    [
        ("/api/products", "get", ["200"]),
        ("/api/products/{id}", "get", ["200", "404"]),
        ("/api/products", "post", ["201", "400", "409"]),
        ("/api/products/{id}", "put", ["204", "400", "404", "409"]),
        ("/api/products/{id}", "delete", ["204", "404", "422"]),
        ("/api/products/{id}/stock", "get", ["200", "404", "503"]),
        ("/api/legacy/products/{id}", "get", ["200", "404"]),
        ("/minimal/products", "get", ["200"]),
        ("/minimal/products/{id}", "get", ["200", "404"]),
        ("/minimal/products", "post", ["201", "400", "409"]),
        ("/minimal/products/{id}", "put", ["204", "400", "404", "409"]),
        ("/minimal/products/{id}", "delete", ["204", "404", "422"]),
        ("/minimal/products/{id}/stock", "get", ["200", "404", "503"]),
    ];

    public static TheoryData<string> Documents() => new() { Hosts.Scalar, Hosts.Swagger };

    private static async Task<JsonElement> LoadDocument(string host)
    {
        var url = host == Hosts.Scalar ? "/openapi/v1.json" : "/swagger/v1/swagger.json";
        var response = await Hosts.Create(host).GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Json();
    }

    private static JsonElement FindPath(JsonElement paths, string path)
    {
        // Route constraints ({id:int}) are written as {id} by both generators.
        foreach (var p in paths.EnumerateObject())
            if (string.Equals(p.Name, path, StringComparison.OrdinalIgnoreCase) || string.Equals(p.Name, path + "/", StringComparison.OrdinalIgnoreCase))
                return p.Value;
        throw new Xunit.Sdk.XunitException($"path {path} not documented");
    }

    [Theory, MemberData(nameof(Documents))]
    public async Task Every_runtime_status_is_documented(string host)
    {
        var paths = (await LoadDocument(host)).GetProperty("paths");
        var missing = new List<string>();

        foreach (var (path, method, statuses) in Operations)
        {
            var responses = FindPath(paths, path).GetProperty(method).GetProperty("responses");
            foreach (var status in statuses)
                if (!responses.TryGetProperty(status, out _))
                    missing.Add($"{method.ToUpperInvariant()} {path} {status}");
        }

        Assert.True(missing.Count == 0, $"{host}: undocumented responses: {string.Join(", ", missing)}");
    }

    [Theory, MemberData(nameof(Documents))]
    public async Task Problem_examples_carry_the_runtime_type(string host)
    {
        var responses = FindPath((await LoadDocument(host)).GetProperty("paths"), "/api/products/{id}")
            .GetProperty("get").GetProperty("responses");

        var problem = responses.GetProperty("404").GetProperty("content").GetProperty("application/problem+json");
        var example = problem.TryGetProperty("example", out var single)
            ? single
            : problem.GetProperty("examples").EnumerateObject().First().Value.GetProperty("value");

        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", example.GetProperty("type").GetString());
    }
}
