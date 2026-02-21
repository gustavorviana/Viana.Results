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
    public void ProblemResult_WithEmptyTitle_DefaultsToError()
    {
        // Act
        var problem = new ProblemResult(400, "  ");

        // Assert
        Assert.Equal("Error", problem.Title);
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
        var problem = ProblemResult.WithDescription(400, "Validation failed", null, "about:blank", extensions);

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
        var problem = ProblemResult.WithDescription(400, title, description);

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
        var problem = ProblemResult.WithDescription(400, "Bad request", "Invalid email format", "about:blank", extensions);

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
        var problem = ProblemResult.WithDescription(404, "Not found");

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
        var problem = ProblemResult.WithDescription(500, "Server error", null, "about:blank", extensions);

        // Assert
        Assert.False(problem.Extensions.ContainsKey("description"));
        Assert.True(problem.Extensions.TryGetValue("traceId", out var traceId));
        Assert.Equal("abc-123", traceId);
    }

    [Fact]
    public void ProblemResult_WithNullDescription_DoesNotAddDescriptionKey()
    {
        // Act
        var problem = ProblemResult.WithDescription(500, "Error");

        // Assert
        Assert.False(problem.Extensions.ContainsKey("description"));
    }
}
