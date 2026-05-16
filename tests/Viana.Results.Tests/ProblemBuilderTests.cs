namespace Viana.Results.Tests;

public class ProblemBuilderTests
{
    [Fact]
    public void Build_WithStatusOnly_PopulatesDefaults()
    {
        var problem = new ProblemBuilder(404).Build();

        Assert.Equal(404, problem.Status);
        Assert.Equal("Error", problem.Title);
        Assert.Equal("about:blank", problem.Type);
        Assert.Empty(problem.Extensions);
    }

    [Fact]
    public void Build_WithTitleTypeDescription_StoresThem()
    {
        var problem = new ProblemBuilder(400)
            .WithTitle("Bad Request")
            .WithType("https://example.com/errors/bad-request")
            .WithDescription("Something is off.")
            .Build();

        Assert.Equal(400, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal("https://example.com/errors/bad-request", problem.Type);
        Assert.Equal("Something is off.", problem.Extensions["description"]);
    }

    [Fact]
    public void Build_WithDetail_StoresUnderRfcKey()
    {
        var problem = new ProblemBuilder(500)
            .WithDetail("Database connection refused at host db-01.")
            .Build();

        Assert.Equal("Database connection refused at host db-01.", problem.Extensions["detail"]);
    }

    [Fact]
    public void Build_WithInstance_StoresUnderRfcKey()
    {
        var problem = new ProblemBuilder(404)
            .WithInstance("/api/users/abc-123")
            .Build();

        Assert.Equal("/api/users/abc-123", problem.Extensions["instance"]);
    }

    [Fact]
    public void WithDetail_Null_RemovesPreviouslySetValue()
    {
        var builder = new ProblemBuilder(500).WithDetail("first");
        Assert.Equal("first", builder.Build().Extensions["detail"]);

        builder.WithDetail(null);
        Assert.False(builder.Build().Extensions.ContainsKey("detail"));
    }

    [Fact]
    public void WithInstance_Null_RemovesPreviouslySetValue()
    {
        var builder = new ProblemBuilder(404).WithInstance("/old");
        Assert.Equal("/old", builder.Build().Extensions["instance"]);

        builder.WithInstance(null);
        Assert.False(builder.Build().Extensions.ContainsKey("instance"));
    }

    [Fact]
    public void AddExtension_AddsCustomMember()
    {
        var problem = new ProblemBuilder(422)
            .WithTitle("Validation failed")
            .AddExtension("traceId", "00-abc-123")
            .AddExtension("errors", new Dictionary<string, string[]> { ["email"] = new[] { "required" } })
            .Build();

        Assert.Equal("00-abc-123", problem.Extensions["traceId"]);
        Assert.IsType<Dictionary<string, string[]>>(problem.Extensions["errors"]);
    }

    [Fact]
    public void AddExtension_OverwritesExistingKey()
    {
        var problem = new ProblemBuilder(500)
            .AddExtension("traceId", "first")
            .AddExtension("traceId", "second")
            .Build();

        Assert.Equal("second", problem.Extensions["traceId"]);
    }

    [Fact]
    public void AddExtension_EmptyKey_Throws()
    {
        var builder = new ProblemBuilder(500);
        Assert.Throws<ArgumentException>(() => builder.AddExtension("", "value"));
    }

    [Fact]
    public void AddExtensions_AddsAllProvidedPairs()
    {
        var problem = new ProblemBuilder(400)
            .AddExtensions(new Dictionary<string, object?>
            {
                ["a"] = 1,
                ["b"] = "two"
            })
            .Build();

        Assert.Equal(1, problem.Extensions["a"]);
        Assert.Equal("two", problem.Extensions["b"]);
    }

    [Fact]
    public void AddExtensions_SkipsEmptyKeys()
    {
        var problem = new ProblemBuilder(400)
            .AddExtensions(new Dictionary<string, object?>
            {
                ["valid"] = 1,
                [""] = "ignored"
            })
            .Build();

        Assert.Single(problem.Extensions);
        Assert.Equal(1, problem.Extensions["valid"]);
    }

    [Fact]
    public void Build_FiltersOutReservedRfcMembers()
    {
        // ProblemResult.NormalizeExtensions strips "type", "title", "status", "extensions"
        var problem = new ProblemBuilder(400)
            .AddExtension("status", 999)
            .AddExtension("type", "bogus")
            .AddExtension("title", "bogus")
            .AddExtension("kept", "yes")
            .Build();

        Assert.Single(problem.Extensions);
        Assert.Equal("yes", problem.Extensions["kept"]);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public void WithStatus_ReplacesStatus()
    {
        var problem = new ProblemBuilder(400).WithStatus(409).Build();
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public void FluentChain_ReturnsSameBuilderInstance()
    {
        var builder = new ProblemBuilder(400);

        Assert.Same(builder, builder.WithTitle("t"));
        Assert.Same(builder, builder.WithType("https://x"));
        Assert.Same(builder, builder.WithStatus(401));
        Assert.Same(builder, builder.WithDescription("d"));
        Assert.Same(builder, builder.WithDetail("d2"));
        Assert.Same(builder, builder.WithInstance("/i"));
        Assert.Same(builder, builder.AddExtension("k", "v"));
        Assert.Same(builder, builder.AddExtensions(new Dictionary<string, object?>()));
    }
}
