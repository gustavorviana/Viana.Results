using System.Net;

namespace Viana.Results.Tests;

public class PagedResultTests
{
    [Fact]
    public void PagedResult_Constructor_WithData_SetsProperties()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var pageNumber = 1;
        var totalPages = 10;

        // Act
        var result = new PagedResult<string>(items, pageNumber, totalPages);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, result.Status);
        Assert.Equal(items, result.Data);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(totalPages, result.TotalPages);
        Assert.Null(result.Problem);
    }

    [Fact]
    public void PagedResult_Constructor_WithProblem_SetsProblem()
    {
        // Arrange
        var problem = new ProblemResult(500, "An error occurred");

        // Act
        var result = new PagedResult<string>([], 0, 0, problem);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal(problem, result.Problem);
        Assert.Equal(0, result.PageNumber);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void PagedResult_ImplementsIPagedResultT()
    {
        // Arrange
        var items = new List<string> { "A", "B" };

        // Act
        var result = new PagedResult<string>(items, 2, 1);

        // Assert
        Assert.IsAssignableFrom<IPagedResult<string>>(result);
    }

    [Fact]
    public void PagedResult_ImplementsIListResultT()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };

        // Act
        var result = new PagedResult<int>(items, 1, 1);

        // Assert
        Assert.IsAssignableFrom<IListResult<int>>(result);
    }

    [Fact]
    public void PagedResult_InheritsFromListResultT()
    {
        // Arrange
        var items = new List<string> { "Test" };

        // Act
        var result = new PagedResult<string>(items, 1, 1);

        // Assert
        Assert.IsAssignableFrom<IResult<IReadOnlyList<string>>>(result);
    }

    [Fact]
    public void PagedResult_WithEmptyCollection_WorksCorrectly()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var result = new PagedResult<string>(items, 0, 0);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, result.Status);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.PageNumber);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void PagedResult_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var items = new List<object>
        {
            new { Id = 1, Name = "First" },
            new { Id = 2, Name = "Second" }
        };

        // Act
        var result = new PagedResult<object>(items, 5, 5);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, result.Status);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(5, result.PageNumber);
        Assert.Equal(5, result.TotalPages);
    }

    [Fact]
    public void PagedResult_PageNumberAndTotalPages_CanDifferFromDataCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var pageNumber = 2;
        var totalPages = 10;

        // Act
        var result = new PagedResult<int>(items, pageNumber, totalPages);

        // Assert
        Assert.Equal(5, result.Data.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.TotalPages);
    }

    [Fact]
    public void PagedResult_Create_CalculatesTotalPages()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var pageNumber = 1;
        var pageSize = 5;
        var totalCount = 47;

        // Act
        var result = PagedResult<int>.Create(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Equal(5, result.Data.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.TotalPages);
    }

    [Fact]
    public void PagedResult_Data_IsEnumerable()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var result = new PagedResult<int>(items, 1, 1);

        // Act
        var filtered = result.Data.Where(x => x > 5).ToList();

        // Assert
        Assert.Equal(5, filtered.Count);
        Assert.Equal(new[] { 6, 7, 8, 9, 10 }, filtered);
    }
}
