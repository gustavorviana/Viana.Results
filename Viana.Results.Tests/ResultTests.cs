using System.Net;

namespace Viana.Results.Tests;

public class ResultTests
{
    [Fact]
    public void Result_WithStatus200_SetsStatusCodeToOK()
    {
        // Act
        var result = new Result(200);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, result.Status);
        Assert.Null(result.Problem);
    }

    [Fact]
    public void ResultT_WithData_SetsDataAndStatusCode()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var result = new Result<object>(data, 200);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, result.Status);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Problem);
    }

    [Fact]
    public void ResultT_WithDataAndStatus_SetsPropertiesCorrectly()
    {
        // Arrange
        var data = "test data";

        // Act
        var result = new Result<string>(data, 200);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, result.Status);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Problem);
    }

    [Fact]
    public void Result_WithProblem_SetsProblemAndStatusCode()
    {
        // Arrange
        var problem = new ProblemResult(500, "An error occurred");

        // Act
        var result = new Result(problem);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal(problem, result.Problem);
    }

    [Fact]
    public void Result_WithProblemResult_SetsPropertiesCorrectly()
    {
        // Arrange
        var problem = new ProblemResult(500, "Error details");

        // Act
        var result = new Result(problem);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal(problem, result.Problem);
        Assert.Equal("Error details", result.Problem!.Title);
    }

    [Fact]
    public void Result_WithStatus_SetsStatusCodeCorrectly()
    {
        // Act
        var result = new Result((int)HttpStatusCode.NotFound, new ProblemResult(404, "Not found"));

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, result.Status);
    }

    [Fact]
    public void ResultT_WithProblem_SetsProblemAndStatusCode()
    {
        // Arrange
        var problem = new ProblemResult(404, "Not found");

        // Act
        var result = new Result<string>(default, 404, problem);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, result.Status);
        Assert.Equal("Not found", result.Problem!.Title);
    }

    [Fact]
    public void ResultT_WithNullData_ReturnsNullData()
    {
        // Act
        var result = new Result<string?>(null, 200);

        // Assert
        Assert.Null(result.Data);
        Assert.Equal(200, result.Status);
    }

    [Fact]
    public void Result_ImplementsIResult()
    {
        // Act
        var result = new Result(200);

        // Assert
        Assert.IsAssignableFrom<IResult>(result);
    }

    [Fact]
    public void ResultT_ImplementsIResultT()
    {
        // Act
        var result = new Result<string>("ok", 200);

        // Assert
        Assert.IsAssignableFrom<IResult<string>>(result);
    }
}
