using System.Net;

namespace Viana.Results.AspNetCore.Tests
{
    public class MessageResultTests
    {
        [Fact]
        public void MessageResult_DefaultConstructor_SetsStatusCodeToOK()
        {
            // Act
            var result = new MessageResult();

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(result.Message);
        }

        [Fact]
        public void MessageResult_WithStatusCode_SetsStatusCode()
        {
            // Act
            var result = new MessageResult(HttpStatusCode.NotFound);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public void MessageResult_WithMessage_SetsMessage()
        {
            // Arrange
            var message = "Operation completed";

            // Act
            var result = new MessageResult { Message = message };

            // Assert
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void MessageResult_Ok_ReturnsOKStatusWithMessage()
        {
            // Arrange
            var message = "Success";

            // Act
            var result = MessageResult.Ok(message);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void MessageResult_Ok_WithoutMessage_ReturnsDefaultMessage()
        {
            // Act
            var result = MessageResult.Ok();

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("OK", result.Message);
        }

        [Fact]
        public void MessageResult_ServiceUnavailable_SetsCorrectStatusAndHeader()
        {
            // Act
            var result = MessageResult.ServiceUnavailable();

            // Assert
            Assert.Equal((int)HttpStatusCode.ServiceUnavailable, result.StatusCode);
            Assert.True(result.Headers.ContainsKey("Retry-After"));
            Assert.Equal("120", result.Headers["Retry-After"].ToString());
        }

        [Fact]
        public void MessageResult_ServiceUnavailable_WithCustomRetryAfter_SetsHeader()
        {
            // Arrange
            var retryAfter = 60;

            // Act
            var result = MessageResult.ServiceUnavailable(retryAfter);

            // Assert
            Assert.Equal((int)HttpStatusCode.ServiceUnavailable, result.StatusCode);
            Assert.Equal("60", result.Headers["Retry-After"].ToString());
        }

        [Fact]
        public void MessageResult_WithError_SetsError()
        {
            // Arrange
            var error = new ResultError("Error occurred");

            // Act
            var result = new MessageResult
            {
                Error = error
            };

            // Assert
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void MessageResult_GetReturnObject_WithMessage_ReturnsObjectWithMessage()
        {
            // Arrange
            var message = "Test message";
            var result = new MessageResult { Message = message };

            // Act
            var returnObject = result.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(result, null);

            // Assert
            Assert.NotNull(returnObject);
        }

        [Fact]
        public void MessageResult_GetReturnObject_WithEmptyMessage_ReturnsObjectWithoutMessage()
        {
            // Arrange
            var result = new MessageResult { Message = string.Empty };

            // Act
            var returnObject = result.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(result, null);

            // Assert
            Assert.NotNull(returnObject);
        }

        [Fact]
        public void MessageResult_InheritsFromActionResultBase()
        {
            // Act
            var result = new MessageResult();

            // Assert
            Assert.IsAssignableFrom<ActionResultBase>(result);
        }

        [Fact]
        public void MessageResult_Headers_IsInitialized()
        {
            // Act
            var result = new MessageResult();

            // Assert
            Assert.NotNull(result.Headers);
        }
    }
}