using System.Net;

namespace Viana.Results.Tests
{
    public class ResultGenericTests
    {
        [Fact]
        public void ResultT_DefaultConstructor_SetsStatusCodeToOK()
        {
            // Act
            var result = new Result<string>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.Data);
            Assert.Null(result.Error);
        }

        [Fact]
        public void ResultT_WithData_SetsDataAndStatusCode()
        {
            // Arrange
            var data = "Test data";

            // Act
            var result = new Result<string>(data, null);

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
            var message = "Operation successful";

            // Act
            var result = new Result<int>(data, message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
            Assert.Equal(message, result.Message);
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
            var message = "Operation failed";

            // Act
            var result = new Result<int>(error, message);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal(error, result.Error);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void ResultT_WithCustomStatusCode_SetsStatusCodeCorrectly()
        {
            // Act
            var result = new Result<string>(HttpStatusCode.NotFound);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
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
            var originalResult = new Result("Success message", "test data");

            // Act
            Result<string> result = originalResult;

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("test data", result.Data);
            Assert.Equal("Success message", result.Message);
        }

        [Fact]
        public void ResultT_ImplicitConversionFromResult_WithError_CreatesResultTWithError()
        {
            // Arrange
            var error = new ResultError("Error occurred");
            var originalResult = new Result(error, "Error message");

            // Act
            Result<string> result = originalResult;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal(error, result.Error);
            Assert.Equal("Error message", result.Message);
        }

        [Fact]
        public void ResultT_ImplementsIResultT()
        {
            // Act
            var result = new Result<string>("test", null);

            // Assert
            Assert.IsAssignableFrom<IResult<string>>(result);
        }

        [Fact]
        public void ResultT_ImplementsIResult()
        {
            // Act
            var result = new Result<string>("test", null);

            // Assert
            Assert.IsAssignableFrom<IResult>(result);
        }

        [Fact]
        public void ResultT_IResultData_ReturnsDataAsObject()
        {
            // Arrange
            var data = "test data";
            var result = new Result<string>(data, null);

            // Act
            var iResult = (IResult)result;

            // Assert
            Assert.Equal(data, iResult.Data);
        }

        [Fact]
        public void ResultT_WithComplexType_WorksCorrectly()
        {
            // Arrange
            var data = new { Id = 1, Name = "Test", Active = true };

            // Act
            var result = new Result<object>(data, null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void ResultT_WithNullData_AllowsNull()
        {
            // Act
            var result = new Result<string>((string)null, null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.Data);
        }
    }
}