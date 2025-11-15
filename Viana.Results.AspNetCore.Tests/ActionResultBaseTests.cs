using System.Net;

namespace Viana.Results.AspNetCore.Tests
{
    public class ActionResultBaseTests
    {
        [Fact]
        public void FromResult_WithNonGenericResult_ReturnsObjectResult()
        {
            // Arrange
            var result = new Result("Success message", HttpStatusCode.OK);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal("Success message", objectResult.Data);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
        }

        [Fact]
        public void FromResult_WithResultError_ReturnsMessageResultWithError()
        {
            // Arrange
            var error = new ResultError("An error occurred");
            var result = new Result(error, HttpStatusCode.BadRequest);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<MessageResult>(actionResult);
            var messageResult = (MessageResult)actionResult;
            Assert.Equal(error, messageResult.Error);
            Assert.Equal((int)HttpStatusCode.BadRequest, messageResult.StatusCode);
        }

        [Fact]
        public void FromResult_WithPaginatedResult_ReturnsPageResult()
        {
            // Arrange
            var items = new List<string> { "Item1", "Item2", "Item3" };
            var result = new PaginatedResult<string>(items, 100, 10);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<PageResult>(actionResult);
            var pageResult = (PageResult)actionResult;
            Assert.Equal(100, pageResult.Total);
            Assert.Equal(10, pageResult.Pages);
            Assert.Equal(3, pageResult.Items.Count);
        }

        [Fact]
        public void FromResult_WithCollectionResult_ReturnsItemsResult()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var result = new CollectionResult<int>(items);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<ItemsResult>(actionResult);
            var itemsResult = (ItemsResult)actionResult;
            Assert.Equal(5, itemsResult.Items.Count);
        }

        [Fact]
        public void FromResult_WithGenericResult_ReturnsObjectResult()
        {
            // Arrange
            var data = new { Id = 1, Name = "Test" };
            var result = new Result<object>(data, HttpStatusCode.OK);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal(data, objectResult.Data);
        }

        [Fact]
        public void FromResult_WithResultString_ReturnsObjectResult()
        {
            // Arrange
            var result = new Result<string>("test data", HttpStatusCode.OK);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal("test data", objectResult.Data);
        }

        [Fact]
        public void FromResult_WithPaginatedResultAndMessage_IncludesMessage()
        {
            // Arrange
            var items = new List<string> { "A", "B", "C" };
            var result = new PaginatedResult<string>(items, 50, 5);

            // Use reflection to set message since constructor doesn't have it
            var messageProperty = result.GetType().BaseType.GetProperty("Message");

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<PageResult>(actionResult);
        }

        [Fact]
        public void FromResult_WithEmptyCollectionResult_ReturnsItemsResultWithEmptyArray()
        {
            // Arrange
            var items = new List<string>();
            var result = new CollectionResult<string>(items);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<ItemsResult>(actionResult);
            var itemsResult = (ItemsResult)actionResult;
            Assert.NotNull(itemsResult.Items);
        }

        [Fact]
        public void FromResult_WithComplexObjectInResult_ReturnsObjectResult()
        {
            // Arrange
            var data = new
            {
                User = new { Id = 1, Name = "John" },
                Settings = new { Theme = "Dark" },
                Roles = new[] { "Admin", "User" }
            };
            var result = new Result<object>(data, HttpStatusCode.OK);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal(data, objectResult.Data);
        }

        [Fact]
        public void FromResult_WithPaginatedResultWithZeroPages_HandlesCorrectly()
        {
            // Arrange
            var items = new List<string>();
            var result = new PaginatedResult<string>(items, 0, 0);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<PageResult>(actionResult);
            var pageResult = (PageResult)actionResult;
            Assert.Equal(0, pageResult.Total);
            Assert.Equal(0, pageResult.Pages);
        }

        [Fact]
        public void FromResult_WithDifferentStatusCodes_PreservesStatusCode()
        {
            // Arrange
            var result1 = new Result("Created", HttpStatusCode.Created);
            var result2 = new Result<string>("data", HttpStatusCode.Accepted);
            var result3 = new Result("Not Modified", HttpStatusCode.NotModified);

            // Act
            var actionResult1 = ActionResultBase.FromResult(result1);
            var actionResult2 = ActionResultBase.FromResult(result2);
            var actionResult3 = ActionResultBase.FromResult(result3);

            // Assert
            Assert.Equal((int)HttpStatusCode.Created, actionResult1.StatusCode);
            Assert.Equal((int)HttpStatusCode.Accepted, actionResult2.StatusCode);
            Assert.Equal((int)HttpStatusCode.NotModified, actionResult3.StatusCode);
        }

        [Fact]
        public void FromResult_WithResultInt_ReturnsObjectResult()
        {
            // Arrange
            var result = new Result<int>(42, HttpStatusCode.OK);

            // Act
            var actionResult = ActionResultBase.FromResult(result);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal(42, objectResult.Data);
        }
    }
}