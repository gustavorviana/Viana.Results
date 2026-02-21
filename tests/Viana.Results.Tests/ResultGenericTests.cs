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
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Equal(data, result.Data);
            Assert.Null(result.Problem);
        }

        [Fact]
        public void ResultT_WithDataAndMessage_SetsPropertiesCorrectly()
        {
            // Arrange
            var data = 42;

            // Act
            var result = new Result<int>(data);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Equal(data, result.Data);
            Assert.Null(result.Problem);
        }

        [Fact]
        public void ResultT_WithError_SetsErrorAndStatusCode()
        {
            // Arrange
            var error = new ProblemResult(500, "An error occurred");

            // Act
            var result = new Result<string>(error);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
            Assert.Equal(error, result.Problem);
        }

        [Fact]
        public void ResultT_WithErrorAndMessage_SetsPropertiesCorrectly()
        {
            // Arrange
            var error = new ProblemResult(500, "Error details");

            // Act
            var result = new Result<int>(error);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
            Assert.Equal(error, result.Problem);
        }

        [Fact]
        public void ResultT_ImplicitConversionFromValue_CreatesResult()
        {
            // Arrange
            string value = "test";

            // Act
            Result<string> result = value;

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Equal(value, result.Data);
        }

        [Fact]
        public void ResultT_ImplicitConversionFromResult_WithSuccess_CreatesResultT()
        {
            // Arrange
            var originalResult = new Result(200);

            // Act
            IResultData result = originalResult;

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, originalResult.Status);
            Assert.Null(result.Data);
        }

        [Fact]
        public void ResultT_ImplicitConversionFromResult_WithError_CreatesResultTWithError()
        {
            // Arrange
            var error = new ProblemResult(500, "Error occurred");
            var originalResult = new Result(error);

            // Act
            Result<string> result = originalResult;

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
            Assert.Equal(error, result.Problem);
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
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void ResultT_WithNullData_AllowsNull()
        {
            // Act
            var result = new Result<string>((string)null);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Null(result.Data);
        }
    }
}