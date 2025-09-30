using System.Net;

namespace Viana.Results.Tests
{
    public class ResultTests
    {
        [Fact]
        public void Result_DefaultConstructor_SetsStatusCodeToOK()
        {
            // Act
            var result = new Result();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.Data);
            Assert.Null(result.Error);
            Assert.Empty(result.Message);
        }

        [Fact]
        public void Result_WithData_SetsDataAndStatusCode()
        {
            // Arrange
            var data = new { Id = 1, Name = "Test" };

            // Act
            var result = new Result(data);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Result_WithDataAndMessage_SetsPropertiesCorrectly()
        {
            // Arrange
            var data = "test data";
            var message = "Operation successful";

            // Act
            var result = new Result(data, message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
            Assert.Equal(message, result.Message);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Result_WithError_SetsErrorAndStatusCode()
        {
            // Arrange
            var error = new ResultError("An error occurred");

            // Act
            var result = new Result(error);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal(error, result.Error);
            Assert.Null(result.Data);
        }

        [Fact]
        public void Result_WithErrorAndMessage_SetsPropertiesCorrectly()
        {
            // Arrange
            var error = new ResultError("Error details");
            var message = "Operation failed";

            // Act
            var result = new Result(error, message);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal(error, result.Error);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void Result_WithCustomStatusCode_SetsStatusCodeCorrectly()
        {
            // Act
            var result = new Result(HttpStatusCode.NotFound);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public void Result_WithMessageAndCustomStatus_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Not found";

            // Act
            var result = new Result(message, HttpStatusCode.NotFound);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void Result_WithNullMessage_ReturnsEmptyString()
        {
            // Act
            var result = new Result((string)null);

            // Assert
            Assert.Null(result.Message);
        }

        [Fact]
        public void Result_WithErrorAndNullMessage_SetsMessageToEmptyString()
        {
            // Arrange
            var error = new ResultError("Error");

            // Act
            var result = new Result(error, null);

            // Assert
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Result_ImplementsIResult()
        {
            // Act
            var result = new Result();

            // Assert
            Assert.IsAssignableFrom<IResult>(result);
        }
    }
}