using System.Net;

namespace Viana.Results.Tests
{
    public class ResultGenericTests
    {
        [Fact]
        public void ResultT_WithData_SetsDataAndStatusCode()
        {
            // Arrange
            var data = "Test data";

            // Act
            var result = new Result<string>(data);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
            Assert.Null(result.Error);
        }

        [Fact]
        public void ResultT_WithDataAndMessage_SetsPropertiesCorrectly()
        {
            // Arrange
            var data = 42;

            // Act
            var result = new Result<int>(data);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
            Assert.Null(result.Error);
        }

        [Fact]
        public void ResultT_WithError_SetsErrorAndStatusCode()
        {
            // Arrange
            var error = new ResultError("An error occurred");

            // Act
            var result = new Result<string>(error);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void ResultT_WithErrorAndMessage_SetsPropertiesCorrectly()
        {
            // Arrange
            var error = new ResultError("Error details");

            // Act
            var result = new Result<int>(error);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void ResultT_ImplicitConversionFromValue_CreatesResult()
        {
            // Arrange
            string value = "test";

            // Act
            Result<string> result = value;

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(value, result.Data);
        }

        [Fact]
        public void ResultT_ImplicitConversionFromResult_WithData_CreatesResultT()
        {
            // Arrange
            var originalResult = new Result("Success message");

            // Act
            Result<string> result = originalResult;

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Success message", result.Data);
        }

        [Fact]
        public void ResultT_ImplicitConversionFromResult_WithError_CreatesResultTWithError()
        {
            // Arrange
            var error = new ResultError("Error occurred");
            var originalResult = new Result(error);

            // Act
            Result<string> result = originalResult;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void ResultT_ImplementsIResultT()
        {
            // Act
            var result = new Result<string>("test");

            // Assert
            Assert.IsAssignableFrom<IResult<string>>(result);
        }

        [Fact]
        public void ResultT_WithComplexType_WorksCorrectly()
        {
            // Arrange
            var data = new { Id = 1, Name = "Test", Active = true };

            // Act
            var result = new Result<object>(data);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void ResultT_WithNullData_AllowsNull()
        {
            // Act
            var result = new Result<string>((string)null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.Data);
        }
    }
}