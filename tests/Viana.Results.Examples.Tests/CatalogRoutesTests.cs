using System.Net;
using System.Net.Http.Json;

namespace Viana.Results.Examples.Tests;

/// <summary>Every catalog route and status, in both styles (controllers and minimal APIs), on both hosts.</summary>
public class CatalogRoutesTests
{
    public static TheoryData<string, string> HostsAndStyles()
    {
        var data = new TheoryData<string, string>();
        foreach (var host in new[] { Hosts.Scalar, Hosts.Swagger })
            foreach (var prefix in new[] { "/api/products", "/minimal/products" })
                data.Add(host, prefix);
        return data;
    }

    private static async Task<System.Text.Json.JsonElement> AssertProblem(HttpResponseMessage response, HttpStatusCode status, string? type = null)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Json();
        Assert.Equal((int)status, body.GetProperty("status").GetInt32());
        if (type is not null)
            Assert.Equal(type, body.GetProperty("type").GetString());
        return body;
    }

    [Theory, MemberData(nameof(HostsAndStyles))]
    public async Task List_returns_the_seed(string host, string prefix)
    {
        var response = await Hosts.Create(host).GetAsync(prefix);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, (await response.Json()).GetArrayLength());
    }

    [Theory, MemberData(nameof(HostsAndStyles))]
    public async Task Get_returns_200_or_404(string host, string prefix)
    {
        var client = Hosts.Create(host);

        var ok = await client.GetAsync($"{prefix}/1");
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal("KB-001", (await ok.Json()).GetProperty("sku").GetString());

        await AssertProblem(await client.GetAsync($"{prefix}/99"), HttpStatusCode.NotFound,
            "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Theory, MemberData(nameof(HostsAndStyles))]
    public async Task Create_returns_201_validation_400_and_409(string host, string prefix)
    {
        var client = Hosts.Create(host);

        var created = await client.PostAsJsonAsync(prefix, Hosts.ValidProduct());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(4, (await created.Json()).GetProperty("id").GetInt32());

        var invalid = await client.PostAsJsonAsync(prefix, new { sku = "", name = "", price = 0, stock = -1 });
        var errors = (await AssertProblem(invalid, HttpStatusCode.BadRequest)).GetProperty("errors");
        foreach (var field in new[] { "sku", "name", "price", "stock" })
            Assert.True(errors.TryGetProperty(field, out _), $"missing error for {field}");

        var malformed = await client.PostAsync(prefix, new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"));
        await AssertProblem(malformed, HttpStatusCode.BadRequest);

        await AssertProblem(await client.PostAsJsonAsync(prefix, Hosts.ValidProduct("kb-001")), HttpStatusCode.Conflict,
            "https://tools.ietf.org/html/rfc9110#section-15.5.10");
    }

    [Theory, MemberData(nameof(HostsAndStyles))]
    public async Task Update_returns_204_404_and_409(string host, string prefix)
    {
        var client = Hosts.Create(host);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"{prefix}/2", Hosts.ValidProduct("MS-002"))).StatusCode);
        await AssertProblem(await client.PutAsJsonAsync($"{prefix}/99", Hosts.ValidProduct()), HttpStatusCode.NotFound);
        await AssertProblem(await client.PutAsJsonAsync($"{prefix}/2", Hosts.ValidProduct("KB-001")), HttpStatusCode.Conflict);
    }

    [Theory, MemberData(nameof(HostsAndStyles))]
    public async Task Delete_returns_204_404_and_422(string host, string prefix)
    {
        var client = Hosts.Create(host);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{prefix}/3")).StatusCode); // seed 3 has no stock
        await AssertProblem(await client.DeleteAsync($"{prefix}/3"), HttpStatusCode.NotFound);
        await AssertProblem(await client.DeleteAsync($"{prefix}/1"), HttpStatusCode.UnprocessableEntity);
    }

    [Theory, MemberData(nameof(HostsAndStyles))]
    public async Task Stock_returns_200_and_503_when_inventory_is_down(string host, string prefix)
    {
        var client = Hosts.Create(host);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"{prefix}/1/stock")).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/inventory/availability?available=false", null)).StatusCode);
        var down = await client.GetAsync($"{prefix}/1/stock");
        var problem = await AssertProblem(down, HttpStatusCode.ServiceUnavailable, "https://tools.ietf.org/html/rfc9110#section-15.6.4");
        Assert.Equal(5, problem.GetProperty("retryAfterSeconds").GetInt32());
    }

    [Theory, MemberData(nameof(HostsAndStyles))]
    public async Task Parallel_creates_with_the_same_sku_yield_one_201_and_one_409(string host, string prefix)
    {
        var client = Hosts.Create(host);

        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => client.PostAsJsonAsync(prefix, Hosts.ValidProduct("DUP-1"))));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(7, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
    }

    [Theory]
    [InlineData(Hosts.Scalar)]
    [InlineData(Hosts.Swagger)]
    public async Task Legacy_mvc_controller_answers_like_the_native_one(string host)
    {
        var client = Hosts.Create(host);

        var ok = await client.GetAsync("/api/legacy/products/1");
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal("KB-001", (await ok.Json()).GetProperty("sku").GetString());

        var missing = await client.GetAsync("/api/legacy/products/99");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("application/problem+json", missing.Content.Headers.ContentType?.MediaType);
    }
}
