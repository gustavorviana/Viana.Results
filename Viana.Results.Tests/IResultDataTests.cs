namespace Viana.Results.Tests;

/// <summary>
/// Tests for <see cref="IResultData"/> interoperability between <see cref="Result"/> and <see cref="Result{T}"/>.
/// Ensures that converting Result&lt;T&gt; to Result does not lose the underlying data.
/// </summary>
public class IResultDataTests
{
    [Fact]
    public void Result_ImplementsIResultData()
    {
        // Act
        var result = new Result(200);

        // Assert
        Assert.IsAssignableFrom<IResultData>(result);
    }

    [Fact]
    public void Result_WithoutData_IResultDataDataIsNull()
    {
        // Arrange
        var result = new Result(200);

        // Act
        var data = ((IResultData)result).Data;

        // Assert
        Assert.Null(data);
    }

    [Fact]
    public void ResultT_ConvertedToResult_PreservesDataViaIResultData()
    {
        // Arrange: Result<string> with value
        Result<string> typedResult = "hello";

        // Act: convert to non-generic Result
        Result result = typedResult;
        var data = ((IResultData)result).Data;

        // Assert: data was not lost
        Assert.NotNull(data);
        Assert.Equal("hello", data);
        Assert.Equal(200, result.Status);
        Assert.Null(result.Problem);
    }

    [Fact]
    public void ResultT_ConvertedToResult_AndBackToResultT_PreservesData()
    {
        // Arrange
        var payload = new { Id = 42, Name = "Test" };
        Result<object> typedResult = payload;

        // Act: Result<T> -> Result -> Result<T>
        Result nonGeneric = typedResult;
        Result<object> backToTyped = nonGeneric;

        // Assert
        Assert.Equal(payload, backToTyped.Data);
        Assert.Equal(200, backToTyped.Status);
        Assert.Null(backToTyped.Problem);
    }

    [Fact]
    public void ResultT_WithValueType_ConvertedToResult_PreservesDataViaIResultData()
    {
        // Arrange
        Result<int> typedResult = 123;

        // Act
        Result result = typedResult;
        var data = ((IResultData)result).Data;

        // Assert
        Assert.NotNull(data);
        Assert.Equal(123, data);
    }

    [Fact]
    public void ResultT_WithProblem_ConvertedToResult_DoesNotExposeDataViaIResultData()
    {
        // Arrange: error result
        var problem = new ProblemResult(404, "Not found");
        var typedResult = new Result<string>(default, 404, problem);

        // Act
        Result result = typedResult;
        var data = ((IResultData)result).Data;

        // Assert: problem is preserved; data is null for error results
        Assert.Equal(404, result.Status);
        Assert.NotNull(result.Problem);
        Assert.Equal("Not found", result.Problem!.Title);
        Assert.Null(data);
    }

    [Fact]
    public void Result_FromResultT_ThenToResultT_SameType_RoundTripPreservesData()
    {
        // Arrange
        Result<string> original = "round-trip";

        // Act
        Result nonGeneric = original;
        Result<string> roundTripped = nonGeneric;

        // Assert
        Assert.Equal("round-trip", roundTripped.Data);
        Assert.Equal(original.Status, roundTripped.Status);
    }

    [Fact]
    public void Result_FromResultT_WithComplexObject_IResultDataDataReturnsSameInstance()
    {
        // Arrange
        var entity = new { Id = 1, Name = "Entity", Tags = new[] { "a", "b" } };
        Result<object> typedResult = entity;

        // Act
        Result result = typedResult;
        var data = ((IResultData)result).Data;

        // Assert
        Assert.NotNull(data);
        Assert.Same(entity, data);
    }
}
