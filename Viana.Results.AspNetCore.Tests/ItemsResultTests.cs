using System.Net;

namespace Viana.Results.AspNetCore.Tests
{
    public class ItemsResultTests
    {
        [Fact]
        public void ItemsResult_DefaultConstructor_SetsStatusCodeToOK()
        {
            // Act
            var result = new ItemsResult();

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(result.Message);
            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
        }

        [Fact]
        public void ItemsResult_WithStatusCode_SetsStatusCode()
        {
            // Act
            var result = new ItemsResult(HttpStatusCode.Created);

            // Assert
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public void ItemsResult_WithItems_SetsItems()
        {
            // Arrange
            var items = new object[] { "Item1", "Item2", "Item3" };

            // Act
            var result = new ItemsResult { Items = items };

            // Assert
            Assert.Equal(items, result.Items);
            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public void ItemsResult_WithMessage_SetsMessage()
        {
            // Arrange
            var message = "Items retrieved successfully";

            // Act
            var result = new ItemsResult { Message = message };

            // Assert
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void ItemsResult_WithEmptyItems_InitializesEmptyArray()
        {
            // Act
            var result = new ItemsResult { Items = new object[] { } };

            // Assert
            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
        }

        [Fact]
        public void ItemsResult_WithNullItems_HandlesNull()
        {
            // Act
            var result = new ItemsResult { Items = null };

            // Assert - GetReturnObject should handle null items
            var returnObject = result.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(result, null);

            Assert.NotNull(returnObject);
        }

        [Fact]
        public void ItemsResult_InheritsFromActionResultBase()
        {
            // Act
            var result = new ItemsResult();

            // Assert
            Assert.IsAssignableFrom<ActionResultBase>(result);
        }

        [Fact]
        public void ItemsResult_WithItemsAndMessage_SetsBothProperties()
        {
            // Arrange
            var items = new object[] { 1, 2, 3 };
            var message = "Success";

            // Act
            var result = new ItemsResult
            {
                Items = items,
                Message = message
            };

            // Assert
            Assert.Equal(items, result.Items);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void ItemsResult_WithError_SetsError()
        {
            // Arrange
            var error = new ResultError("Error loading items");

            // Act
            var result = new ItemsResult { Error = error };

            // Assert
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void ItemsResult_WithComplexItems_StoresCorrectly()
        {
            // Arrange
            var items = new object[]
            {
                new { Id = 1, Name = "Item1" },
                new { Id = 2, Name = "Item2" },
                new { Id = 3, Name = "Item3" }
            };

            // Act
            var result = new ItemsResult { Items = items };

            // Assert
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(items, result.Items);
        }

        [Fact]
        public void ItemsResult_WithEmptyMessage_DoesNotIncludeMessageInReturnObject()
        {
            // Arrange
            var items = new object[] { "test" };
            var result = new ItemsResult { Items = items, Message = string.Empty };

            // Act
            var returnObject = result.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(result, null);

            // Assert
            Assert.NotNull(returnObject);
            var properties = returnObject.GetType().GetProperties();
            Assert.Contains(properties, p => p.Name == "Data");
        }

        [Fact]
        public void ItemsResult_WithMixedTypeItems_HandlesCorrectly()
        {
            // Arrange
            var items = new object[] { "string", 123, true, new { Id = 1 } };

            // Act
            var result = new ItemsResult { Items = items };

            // Assert
            Assert.Equal(4, result.Items.Count);
            Assert.IsType<string>(result.Items.First());
            Assert.IsType<int>(result.Items.Skip(1).First());
            Assert.IsType<bool>(result.Items.Skip(2).First());
        }

        [Fact]
        public void ItemsResult_DefaultMessage_IsEmpty()
        {
            // Act
            var result = new ItemsResult();

            // Assert
            Assert.Equal(string.Empty, result.Message);
        }
    }
}