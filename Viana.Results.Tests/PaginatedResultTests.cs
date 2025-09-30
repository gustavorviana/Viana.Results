using System.Net;

namespace Viana.Results.Tests
{
    public class PaginatedResultTests
    {
        [Fact]
        public void PaginatedResult_Constructor_WithData_SetsProperties()
        {
            // Arrange
            var items = new List<string> { "Item1", "Item2", "Item3" };
            var totalCount = 100;
            var pages = 10;

            // Act
            var result = new PaginatedResult<string>(items, totalCount, pages);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(items, result.Data);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(pages, result.Pages);
            Assert.Null(result.Error);
        }

        [Fact]
        public void PaginatedResult_Constructor_WithError_SetsError()
        {
            // Arrange
            var error = new ResultError("An error occurred");
            var message = "Failed to retrieve data";

            // Act
            var result = new PaginatedResult<string>(error, message);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal(error, result.Error);
            Assert.Equal(message, result.Message);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.Pages);
        }

        [Fact]
        public void PaginatedResult_AsQueryable_WithData_ReturnsQueryable()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var result = new PaginatedResult<int>(items, 5, 1);

            // Act
            var queryable = result.AsQueryable();

            // Assert
            Assert.NotNull(queryable);
            Assert.Equal(5, queryable.Count());
            Assert.IsAssignableFrom<IQueryable<int>>(queryable);
        }

        [Fact]
        public void PaginatedResult_AsQueryable_WithNullData_ReturnsEmptyQueryable()
        {
            // Arrange
            var error = new ResultError("Error");
            var result = new PaginatedResult<string>(error);

            // Act
            var queryable = result.AsQueryable();

            // Assert
            Assert.NotNull(queryable);
            Assert.Empty(queryable);
        }

        [Fact]
        public void PaginatedResult_ImplementsIPaginatedResult()
        {
            // Arrange
            var items = new List<string> { "A", "B" };

            // Act
            var result = new PaginatedResult<string>(items, 2, 1);

            // Assert
            Assert.IsAssignableFrom<IPaginatedResult>(result);
        }

        [Fact]
        public void PaginatedResult_ImplementsICollectionResult()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };

            // Act
            var result = new PaginatedResult<int>(items, 3, 1);

            // Assert
            Assert.IsAssignableFrom<ICollectionResult>(result);
        }

        [Fact]
        public void PaginatedResult_InheritsFromResultT()
        {
            // Arrange
            var items = new List<string> { "Test" };

            // Act
            var result = new PaginatedResult<string>(items, 1, 1);

            // Assert
            Assert.IsAssignableFrom<Result<ICollection<string>>>(result);
        }

        [Fact]
        public void PaginatedResult_WithEmptyCollection_WorksCorrectly()
        {
            // Arrange
            var items = new List<string>();

            // Act
            var result = new PaginatedResult<string>(items, 0, 0);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty((ICollection<string>)result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.Pages);
        }

        [Fact]
        public void PaginatedResult_WithComplexType_WorksCorrectly()
        {
            // Arrange
            var items = new List<object>
            {
                new { Id = 1, Name = "First" },
                new { Id = 2, Name = "Second" }
            };

            // Act
            var result = new PaginatedResult<object>(items, 50, 5);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(50, result.TotalCount);
            Assert.Equal(5, result.Pages);
        }

        [Fact]
        public void PaginatedResult_TotalCountAndPages_CanBeDifferentFromDataCount()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 }; // 5 items per page
            var totalCount = 47; // total items in database
            var pages = 10; // total pages

            // Act
            var result = new PaginatedResult<int>(items, totalCount, pages);

            // Assert
            Assert.Equal(5, result.Data.Count); // Current page has 5 items
            Assert.Equal(47, result.TotalCount); // Total items
            Assert.Equal(10, result.Pages); // Total pages
        }

        [Fact]
        public void PaginatedResult_AsQueryable_CanBeFiltered()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var result = new PaginatedResult<int>(items, 10, 1);

            // Act
            var queryable = result.AsQueryable();
            var filtered = queryable.Where(x => x > 5).ToList();

            // Assert
            Assert.Equal(5, filtered.Count);
            Assert.Equal(new[] { 6, 7, 8, 9, 10 }, filtered);
        }
    }
}