namespace Viana.Results.OpenApi.Tests.DefaultResult;

/// <summary>
/// Feature 011: a bare Result documents 204 (no inferred 200), hand-declared successes win, and
/// WithoutDefaultResult keeps only what was registered by hand. Every case runs on both generators.
/// </summary>
public class DefaultResultTests
{
    public static TheoryData<string> Generators() => DefaultResultHost.Generators();

    // ---------- US1: bodyless result is 204 ----------

    [Theory, MemberData(nameof(Generators))]
    public async Task Bare_result_documents_204_and_no_200(string generator)
    {
        var doc = await DefaultResultHost.Document(generator);

        foreach (var (path, method) in new[] { ("/ctl/bare", "get"), ("/min/bare", "get") })
        {
            var statuses = doc.Statuses(path, method);
            Assert.Contains("204", statuses);
            Assert.DoesNotContain("200", statuses);
            Assert.False(doc.Response(path, method, "204").HasContent());
            Assert.Contains("404", statuses); // (f) problems unchanged
            Assert.Contains("500", statuses); // (f) global example unchanged
        }
    }

    [Theory, MemberData(nameof(Generators))]
    public async Task Bare_result_with_Produces_204_lists_204_once(string generator)
    {
        var statuses = (await DefaultResultHost.Document(generator)).Statuses("/min/bare-produces", "put");

        Assert.Contains("204", statuses);
        Assert.DoesNotContain("200", statuses);
    }

    [Theory, MemberData(nameof(Generators))]
    public async Task Hand_declared_202_replaces_the_default(string generator)
    {
        var doc = await DefaultResultHost.Document(generator);

        foreach (var (path, method) in new[] { ("/ctl/bare-202", "get"), ("/min/bare-202", "post") })
        {
            var successes = doc.Statuses(path, method).Where(s => s.StartsWith('2')).ToArray();
            Assert.Equal(["202"], successes);
        }
    }

    [Theory, MemberData(nameof(Generators))]
    public async Task Typed_result_keeps_200_with_its_schema(string generator)
    {
        var doc = await DefaultResultHost.Document(generator);

        foreach (var (path, method) in new[] { ("/ctl/typed", "get"), ("/min/typed", "get") })
        {
            Assert.Contains("200", doc.Statuses(path, method));
            Assert.True(doc.Response(path, method, "200").HasContent());
        }
    }

    [Theory, MemberData(nameof(Generators))]
    public async Task Example_status_is_a_declared_success(string generator)
    {
        var doc = await DefaultResultHost.Document(generator);

        // (g) ProducesResult of Results.Ok() → 204
        Assert.Equal(["204"], doc.Statuses("/min/empty-example", "delete").Where(s => s.StartsWith('2')).ToArray());
        // (g2) controller Result<T> with a Created example → only 201, with content
        Assert.Equal(["201"], doc.Statuses("/ctl/created", "post").Where(s => s.StartsWith('2')).ToArray());
        Assert.True(doc.Response("/ctl/created", "post", "201").HasContent());
        // (g3) minimal Result<T> with a 202 example → only 202
        Assert.Equal(["202"], doc.Statuses("/min/typed-202", "post").Where(s => s.StartsWith('2')).ToArray());
    }

    // ---------- US2: WithoutDefaultResult ----------

    [Theory, MemberData(nameof(Generators))]
    public async Task WithoutDefaultResult_documents_exactly_the_hand_registered_successes(string generator)
    {
        var doc = await DefaultResultHost.Document(generator);

        foreach (var (path, method) in new[] { ("/ctl/multi", "get"), ("/min/multi", "get") })
        {
            Assert.Equal(["200", "202"], doc.Statuses(path, method).Where(s => s.StartsWith('2')).ToArray());
            Assert.True(doc.Response(path, method, "200").HasContent());
            Assert.True(doc.Response(path, method, "202").HasContent());
        }
        Assert.Contains("404", doc.Statuses("/min/multi", "get"));
    }

    [Theory, MemberData(nameof(Generators))]
    public async Task WithoutDefaultResult_with_nothing_declared_has_no_success(string generator)
    {
        var doc = await DefaultResultHost.Document(generator);

        foreach (var (path, method) in new[]
                 {
                     ("/ctl/none", "get"), ("/min/none", "get"),
                     ("/marked/bare", "get"), ("/marked/typed", "get"),            // (j) on the controller class
                     ("/min/group/bare", "get"), ("/min/group/typed", "get")       // (l) on a route group
                 })
        {
            Assert.DoesNotContain(doc.Statuses(path, method), s => s.StartsWith('2'));
        }
        Assert.Contains("404", doc.Statuses("/ctl/none", "get"));
        Assert.Contains("404", doc.Statuses("/min/none", "get"));
    }
}
