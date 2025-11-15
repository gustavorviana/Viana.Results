using System.Net;

namespace Viana.Results.Tests
{
    public class CollectionResultTests
    {
        [Fact]
        public void CollectionResult_Constructor_WithData_SetsData()
        {
            // Arrange
            var items = new List<string> { "Item1", "Item2", "Item3" };

            // Act
            var result = new CollectionResult<string>(items);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(items, result.Data);
            Assert.Null(result.Error);
        }

        [Fact]
        public void CollectionResult_ImplementsICollectionResult()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };

            // Act
            var result = new CollectionResult<int>(items);

            // Assert
            Assert.IsAssignableFrom<ICollectionResult>(result);
        }

        [Fact]
        public void CollectionResult_InheritsFromResultT()
        {
            // Arrange
            var items = new List<string> { "Test" };

            // Act
            var result = new CollectionResult<string>(items);

            // Assert
            Assert.IsAssignableFrom<IResult<IReadOnlyList<string>>>(result);
        }

        [Fact]
        public void CollectionResult_IResultData_ReturnsData()
        {
            // Arrange
            var items = new List<string> { "A", "B", "C" };
            var result = new CollectionResult<string>(items);

            // Act
            var iResult = (IResult)result;

            // Assert
            Assert.Equal(items, iResult.Data);
        }

        [Fact]
        public void CollectionResult_WithEmptyCollection_WorksCorrectly()
        {
            // Arrange
            var items = new List<string>();

            // Act
            var result = new CollectionResult<string>(items);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty((ICollection<string>)result.Data);
            Assert.Null(result.Error);
        }

        [Fact]
        public void CollectionResult_WithComplexType_WorksCorrectly()
        {
            // Arrange
            var items = new List<object>
            {
                new { Id = 1, Name = "First" },
                new { Id = 2, Name = "Second" },
                new { Id = 3, Name = "Third" }
            };

            // Act
            var result = new CollectionResult<object>(items);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(items, result.Data);
        }

        [Fact]
        public void CollectionResult_WithNullCollection_CreatesEmptyData()
        {
            // Arrange
            List<string> items = null;

            // Act
            var result = new CollectionResult<string>(items);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public void CollectionResult_ImplementsIResultT()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            var result = new CollectionResult<int>(items);

            // Assert
            Assert.IsAssignableFrom<IResult<IReadOnlyList<int>>>(result);
            var typedResult = (IResult<IReadOnlyList<int>>)result;
            Assert.Equal(items, typedResult.Data);
        }
    }
}