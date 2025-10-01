using System.Net;

namespace Viana.Results.AspNetCore.Tests
{
    public class ObjectResultTests
    {
        [Fact]
        public void ObjectResult_DefaultConstructor_SetsStatusCodeToOK()
        {
            // Act
            var result = new ObjectResult();

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void ObjectResult_WithStatusCode_SetsStatusCode()
        {
            // Act
            var result = new ObjectResult(HttpStatusCode.Created);

            // Assert
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public void ObjectResult_WithResult_SetsResult()
        {
            // Arrange
            var data = new { Id = 1, Name = "Test" };

            // Act
            var result = new ObjectResult { Data = data };

            // Assert
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void ObjectResult_ForError_CreatesObjectResultWithError()
        {
            // Arrange
            var errorData = new { Field = "Email", Message = "Invalid format" };

            // Act
            var result = ObjectResult.ForError(HttpStatusCode.BadRequest, errorData);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(errorData, result.Data);
            Assert.NotNull(result.Error);
            Assert.IsType<DataResultError>(result.Error);
        }

        [Fact]
        public void ObjectResult_ForError_SetsDataResultError()
        {
            // Arrange
            var errorData = "Error details";

            // Act
            var result = ObjectResult.ForError(HttpStatusCode.InternalServerError, errorData);

            // Assert
            var dataError = Assert.IsType<DataResultError>(result.Error);
            Assert.Equal(errorData, dataError.Data);
        }

        [Fact]
        public void ObjectResult_WithError_SetsError()
        {
            // Arrange
            var error = new ResultError("An error occurred");

            // Act
            var result = new ObjectResult { Error = error };

            // Assert
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void ObjectResult_InheritsFromActionResultBase()
        {
            // Act
            var result = new ObjectResult();

            // Assert
            Assert.IsAssignableFrom<ActionResultBase>(result);
        }

        [Fact]
        public void ObjectResult_WithResult_SetsProperties()
        {
            // Arrange
            var data = new { Count = 10 };

            // Act
            var result = new ObjectResult
            {
                Data = data
            };

            // Assert
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void ObjectResult_WithNullResult_AllowsNull()
        {
            // Act
            var result = new ObjectResult { Data = null };

            // Assert
            Assert.Null(result.Data);
        }

        [Fact]
        public void ObjectResult_ForError_WithDifferentStatusCodes_SetsCorrectStatus()
        {
            // Arrange & Act
            var badRequest = ObjectResult.ForError(HttpStatusCode.BadRequest, "Bad data");
            var notFound = ObjectResult.ForError(HttpStatusCode.NotFound, "Not found");
            var forbidden = ObjectResult.ForError(HttpStatusCode.Forbidden, "No access");

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequest.StatusCode);
            Assert.Equal((int)HttpStatusCode.NotFound, notFound.StatusCode);
            Assert.Equal((int)HttpStatusCode.Forbidden, forbidden.StatusCode);
        }

        [Fact]
        public void ObjectResult_WithComplexObject_SetsResult()
        {
            // Arrange
            var complexData = new
            {
                User = new { Id = 1, Name = "John" },
                Settings = new { Theme = "Dark", Language = "en" },
                Permissions = new[] { "read", "write" }
            };

            // Act
            var result = new ObjectResult { Data = complexData };

            // Assert
            Assert.Equal(complexData, result.Data);
        }
    }
}