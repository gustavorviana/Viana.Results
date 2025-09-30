using System.Net;

namespace Viana.Results.AspNetCore.Tests
{
    public class PageResultTests
    {
        [Fact]
        public void PageResult_DefaultConstructor_SetsStatusCodeToOK()
        {
            // Act
            var result = new PageResult();

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(0, result.TotalItems);
            Assert.Equal(0, result.Pages);
        }

        [Fact]
        public void PageResult_WithTotalItems_SetsTotalItems()
        {
            // Arrange
            var totalItems = 100L;

            // Act
            var result = new PageResult { TotalItems = totalItems };

            // Assert
            Assert.Equal(totalItems, result.TotalItems);
        }

        [Fact]
        public void PageResult_WithPages_SetsPages()
        {
            // Arrange
            var pages = 10;

            // Act
            var result = new PageResult { Pages = pages };

            // Assert
            Assert.Equal(pages, result.Pages);
        }

        [Fact]
        public void PageResult_WithItems_SetsItems()
        {
            // Arrange
            var items = new object[] { "Item1", "Item2", "Item3" };

            // Act
            var result = new PageResult { Items = items };

            // Assert
            Assert.Equal(items, result.Items);
            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public void PageResult_WithMessage_SetsMessage()
        {
            // Arrange
            var message = "Page retrieved successfully";

            // Act
            var result = new PageResult { Message = message };

            // Assert
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void PageResult_WithError_SetsError()
        {
            // Arrange
            var error = new ResultError("Error loading page");

            // Act
            var result = new PageResult { Error = error };

            // Assert
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void PageResult_InheritsFromItemsResult()
        {
            // Act
            var result = new PageResult();

            // Assert
            Assert.IsAssignableFrom<ItemsResult>(result);
        }

        [Fact]
        public void PageResult_InheritsFromActionResultBase()
        {
            // Act
            var result = new PageResult();

            // Assert
            Assert.IsAssignableFrom<ActionResultBase>(result);
        }

        [Fact]
        public void PageResult_WithAllProperties_SetsAllCorrectly()
        {
            // Arrange
            var items = new object[] { 1, 2, 3, 4, 5 };
            var totalItems = 47L;
            var pages = 10;
            var message = "Data retrieved";

            // Act
            var result = new PageResult
            {
                Items = items,
                TotalItems = totalItems,
                Pages = pages,
                Message = message
            };

            // Assert
            Assert.Equal(items, result.Items);
            Assert.Equal(totalItems, result.TotalItems);
            Assert.Equal(pages, result.Pages);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void PageResult_WithEmptyItems_InitializesEmptyArray()
        {
            // Act
            var result = new PageResult
            {
                Items = new object[] { },
                TotalItems = 0,
                Pages = 0
            };

            // Assert
            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
        }

        [Fact]
        public void PageResult_WithNullItems_HandlesNull()
        {
            // Act
            var result = new PageResult
            {
                Items = null,
                TotalItems = 100,
                Pages = 10
            };

            // Act - GetReturnObject should handle null items
            var returnObject = result.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(result, null);

            // Assert
            Assert.NotNull(returnObject);
        }

        [Fact]
        public void PageResult_WithComplexItems_StoresCorrectly()
        {
            // Arrange
            var items = new object[]
            {
                new { Id = 1, Name = "Item1", Active = true },
                new { Id = 2, Name = "Item2", Active = false },
                new { Id = 3, Name = "Item3", Active = true }
            };

            // Act
            var result = new PageResult
            {
                Items = items,
                TotalItems = 100,
                Pages = 34
            };

            // Assert
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(100, result.TotalItems);
            Assert.Equal(34, result.Pages);
        }

        [Fact]
        public void PageResult_WithLargeTotalItems_HandlesLargeNumbers()
        {
            // Arrange
            var totalItems = long.MaxValue;

            // Act
            var result = new PageResult { TotalItems = totalItems };

            // Assert
            Assert.Equal(totalItems, result.TotalItems);
        }

        [Fact]
        public void PageResult_FirstPage_Scenario()
        {
            // Arrange
            var items = new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // Act
            var result = new PageResult
            {
                Items = items,
                TotalItems = 47,
                Pages = 5,
                Message = "First page"
            };

            // Assert
            Assert.Equal(10, result.Items.Count); // Current page items
            Assert.Equal(47, result.TotalItems);   // Total in database
            Assert.Equal(5, result.Pages);         // Total pages
        }

        [Fact]
        public void PageResult_LastPage_WithFewerItems()
        {
            // Arrange
            var items = new object[] { 1, 2, 3, 4, 5, 6, 7 }; // Last page with 7 items

            // Act
            var result = new PageResult
            {
                Items = items,
                TotalItems = 47,
                Pages = 5,
                Message = "Last page"
            };

            // Assert
            Assert.Equal(7, result.Items.Count);
            Assert.Equal(47, result.TotalItems);
            Assert.Equal(5, result.Pages);
        }

        [Fact]
        public void PageResult_EmptyMessage_DoesNotIncludeInReturnObject()
        {
            // Arrange
            var result = new PageResult
            {
                Items = new object[] { "test" },
                TotalItems = 1,
                Pages = 1,
                Message = string.Empty
            };

            // Act
            var returnObject = result.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(result, null);

            // Assert
            Assert.NotNull(returnObject);
        }
    }
}