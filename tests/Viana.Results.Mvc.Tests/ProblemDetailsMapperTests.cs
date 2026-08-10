using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Viana.Results.Mvc.Tests;

public class ProblemDetailsMapperTests
{
    [Fact]
    public void ToProblemDetails_MapsStandardMembers()
    {
        var problem = new ProblemResult(404, "Not Found", "https://errors/not-found");

        var details = problem.ToProblemDetails();

        Assert.Equal(404, details.Status);
        Assert.Equal("Not Found", details.Title);
        Assert.Equal("https://errors/not-found", details.Type);
    }

    [Fact]
    public void ToProblemDetails_MapsDescriptionToDetail()
    {
        var problem = ProblemResult.WithDescription(422, "Validation Error", "Invalid input");

        var details = problem.ToProblemDetails();

        Assert.Equal("Invalid input", details.Detail);
        Assert.False(details.Extensions.ContainsKey("description"));
    }

    [Fact]
    public void ToProblemDetails_PrefersDetailExtensionOverDescription()
    {
        var problem = new ProblemBuilder(400)
            .WithTitle("Bad Request")
            .WithDescription("a description")
            .WithDetail("the detail")
            .Build();

        var details = problem.ToProblemDetails();

        Assert.Equal("the detail", details.Detail);
        Assert.False(details.Extensions.ContainsKey("detail"));
        Assert.False(details.Extensions.ContainsKey("description"));
    }

    [Fact]
    public void ToProblemDetails_MapsInstance()
    {
        var problem = new ProblemBuilder(409)
            .WithTitle("Conflict")
            .WithInstance("/orders/42")
            .Build();

        var details = problem.ToProblemDetails();

        Assert.Equal("/orders/42", details.Instance);
        Assert.False(details.Extensions.ContainsKey("instance"));
    }

    [Fact]
    public void ToProblemDetails_CopiesRemainingExtensions()
    {
        var errors = new Dictionary<string, object?> { ["email"] = new[] { "is required" } };
        var problem = new ProblemBuilder(400)
            .WithTitle("Validation Failed")
            .AddExtensions(errors)
            .Build();

        var details = problem.ToProblemDetails();

        Assert.True(details.Extensions.ContainsKey("email"));
    }
}
