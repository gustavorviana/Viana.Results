namespace Viana.Results.Tests;

public class ProblemResultTests
{
    [Fact]
    public void ProblemResult_Constructor_SetsTitleAndStatus()
    {
        // Arrange
        var title = "An error occurred";
        var status = 500;

        // Act
        var problem = new ProblemResult(status, title);

        // Assert
        Assert.Equal(title, problem.Title);
        Assert.Equal(status, problem.Status);
        Assert.Equal("about:blank", problem.Type);
    }

    [Fact]
    public void ProblemResult_WithEmptyTitle_DefaultsToReasonPhrase()
    {
        // Act
        var problem = new ProblemResult(400, "  ");

        // Assert
        Assert.Equal("Bad Request", problem.Title);
    }

    [Theory]
    [InlineData(null, 404, "Not Found")]
    [InlineData("", 404, "Not Found")]
    [InlineData("  ", 500, "Internal Server Error")]
    public void ProblemResult_BlankTitle_DefaultsToReasonPhrase(string? title, int status, string expected)
    {
        var problem = new ProblemResult(status, title!);

        Assert.Equal(expected, problem.Title);
    }

    [Fact]
    public void WithDescription_BlankTitle_DefaultsToReasonPhrase()
    {
        #pragma warning disable CS0618 // exercises the obsolete "description" API until it is removed in 4.0.0
        var problem = ProblemResult.WithDescription(409, "", "Already exists");
        #pragma warning restore CS0618

        Assert.Equal("Conflict", problem.Title);
    }

    [Theory]
    [InlineData(306)]
    [InlineData(418)]
    [InlineData(499)]
    [InlineData(0)]
    [InlineData(-1)]
    public void ProblemResult_BlankTitle_UnknownOrPlaceholderStatus_FallsBackToError(int status)
    {
        var problem = new ProblemResult(status, "");

        Assert.Equal("Error", problem.Title);
    }

    [Theory]
    [InlineData(404)]
    [InlineData(499)]
    public void ProblemResult_ExplicitTitle_IsNeverChanged(int status)
    {
        var problem = new ProblemResult(status, "Order already shipped");

        Assert.Equal("Order already shipped", problem.Title);
    }

    [Fact]
    public void ProblemResult_CustomType_BlankTitle_UsesReasonPhraseAndKeepsType()
    {
        var problem = new ProblemResult(404, "", "https://example.com/probs/x");

        Assert.Equal("Not Found", problem.Title);
        Assert.Equal("https://example.com/probs/x", problem.Type);
    }

    [Fact]
    public void WithDetail_StoresRfc9457DetailMember()
    {
        var problem = ProblemResult.WithDetail(400, "Bad Request", "Email is invalid.");

        Assert.Equal("Email is invalid.", problem.Extensions["detail"]);
        Assert.False(problem.Extensions.ContainsKey("description"));
    }

    [Fact]
    public void WithDetail_KeepsOtherExtensions()
    {
        var extensions = new Dictionary<string, object?> { ["traceId"] = "t-1" };

        var problem = ProblemResult.WithDetail(400, "Bad Request", "x", "about:blank", extensions);

        Assert.Equal("x", problem.Extensions["detail"]);
        Assert.Equal("t-1", problem.Extensions["traceId"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WithDetail_EmptyDetail_IsNotStored(string? detail)
    {
        var problem = ProblemResult.WithDetail(404, "", detail);

        Assert.False(problem.Extensions.ContainsKey("detail"));
        Assert.Equal("Not Found", problem.Title);
    }

    [Fact]
    public void WithDetail_OverridesDetailPassedInExtensions()
    {
        var extensions = new Dictionary<string, object?> { ["detail"] = "old" };

        var problem = ProblemResult.WithDetail(400, "Bad Request", "new", "about:blank", extensions);

        Assert.Equal("new", problem.Extensions["detail"]);
    }

    [Fact]
    public void ProblemResult_BlankType_DefaultsToAboutBlank()
    {
        var problem = new ProblemResult(404, "Missing", "  ");

        Assert.Equal("about:blank", problem.Type);
    }

    [Fact]
    public void ProblemResult_WithExtensions_StoresInExtensions()
    {
        // Arrange
        var extensions = new Dictionary<string, object?>
        {
            { "errors", new Dictionary<string, string[]> { { "Email", new[] { "Invalid" } } } }
        };

        // Act
        var problem = new ProblemResult(400, "Validation failed", "about:blank", extensions);

        // Assert
        Assert.True(problem.Extensions.ContainsKey("errors"));
    }

    [Fact]
    public void ProblemResult_WithCustomType_SetsType()
    {
        // Act
        var problem = new ProblemResult(404, "Not found", "https://api.example.com/errors/not-found");

        // Assert
        Assert.Equal("https://api.example.com/errors/not-found", problem.Type);
    }

    [Fact]
    public void ProblemResult_WithDescription_StoresInExtensions()
    {
        // Arrange
        var title = "Validation failed";
        var description = "The request body contains invalid fields. See 'errors' for details.";

        // Act
        #pragma warning disable CS0618 // exercises the obsolete "description" API until it is removed in 4.0.0
        var problem = ProblemResult.WithDescription(400, title, description);
        #pragma warning restore CS0618

        // Assert
        Assert.True(problem.Extensions.TryGetValue("description", out var desc));
        Assert.Equal(description, desc);
        Assert.Equal(title, problem.Title);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public void ProblemResult_WithDescriptionAndExtensions_IncludesBoth()
    {
        // Arrange
        var extensions = new Dictionary<string, object?> { ["field"] = "Email" };

        // Act
        #pragma warning disable CS0618 // exercises the obsolete "description" API until it is removed in 4.0.0
        var problem = ProblemResult.WithDescription(400, "Bad request", "Invalid email format", "about:blank", extensions);
        #pragma warning restore CS0618

        // Assert
        Assert.True(problem.Extensions.TryGetValue("description", out var desc));
        Assert.Equal("Invalid email format", desc);
        Assert.True(problem.Extensions.TryGetValue("field", out var field));
        Assert.Equal("Email", field);
    }

    [Fact]
    public void ProblemResult_Create_ReturnsInstanceWithoutDescription()
    {
        // Act
        #pragma warning disable CS0618 // exercises the obsolete "description" API until it is removed in 4.0.0
        var problem = ProblemResult.WithDescription(404, "Not found");
        #pragma warning restore CS0618

        // Assert
        Assert.False(problem.Extensions.ContainsKey("description"));
        Assert.Equal("Not found", problem.Title);
        Assert.Equal(404, problem.Status);
        Assert.Equal("about:blank", problem.Type);
    }

    [Fact]
    public void ProblemResult_Create_WithTypeAndExtensions_StoresExtensions()
    {
        // Arrange
        var extensions = new Dictionary<string, object?> { ["traceId"] = "abc-123" };

        // Act
        var problem = new ProblemResult(500, "Server error", "about:blank", extensions);

        // Assert
        Assert.False(problem.Extensions.ContainsKey("description"));
        Assert.True(problem.Extensions.TryGetValue("traceId", out var traceId));
        Assert.Equal("abc-123", traceId);
    }

    [Fact]
    public void ProblemResult_WithNullDescription_DoesNotAddDescriptionKey()
    {
        // Act
        #pragma warning disable CS0618 // exercises the obsolete "description" API until it is removed in 4.0.0
        var problem = ProblemResult.WithDescription(500, "Error");
        #pragma warning restore CS0618

        // Assert
        Assert.False(problem.Extensions.ContainsKey("description"));
    }

    [Fact]
    public void Extensions_WhenEmpty_AreReadOnly()
    {
        Assert.IsNotType<Dictionary<string, object?>>(new ProblemResult(404, "x").Extensions);
    }

    [Fact]
    public void Extensions_WithDetail_AreReadOnly()
    {
        Assert.IsNotType<Dictionary<string, object?>>(ProblemResult.WithDetail(400, "x", "d").Extensions);
    }

    [Fact]
    public void Extensions_NamesDifferingByCase_LastValueWinsFirstCasingKept()
    {
        var input = new Dictionary<string, object?>(StringComparer.Ordinal) { ["errorCode"] = "E1", ["ErrorCode"] = "E2" };

        var problem = new ProblemResult(400, "x", "about:blank", input);

        var single = Assert.Single(problem.Extensions);
        Assert.Equal("errorCode", single.Key);
        Assert.Equal("E2", single.Value);
        Assert.True(problem.Extensions.ContainsKey("ERRORCODE"));
    }
}
