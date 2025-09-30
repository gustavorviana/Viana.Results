using System.Net;

namespace Viana.Results.Tests
{
    public class ResultsHelperTests
    {
        [Fact]
        public void Results_Success_WithMessage_ReturnsSuccessResult()
        {
            // Arrange
            var message = "Operation completed successfully";

            // Act
            var result = Results.Success(message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(message, result.Message);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Results_Success_WithData_ReturnsSuccessResultWithData()
        {
            // Arrange
            var data = new { Id = 1, Name = "Test" };

            // Act
            var result = Results.Success(data);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Results_Success_WithDataAndMessage_ReturnsSuccessResultWithBoth()
        {
            // Arrange
            var data = "test data";
            var message = "Success message";

            // Act
            var result = Results.Success(data, message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
            Assert.Equal(message, result.Message);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Results_Success_WithoutParameters_ReturnsSuccessResult()
        {
            // Act
            var result = Results.Success();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.Data);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Results_Failure_WithMessage_ReturnsFailureResult()
        {
            // Arrange
            var message = "Operation failed";

            // Act
            var result = Results.Failure(message);

            // Assert
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal(message, result.Error.Message);
        }

        [Fact]
        public void Results_Failure_WithMessageAndCustomStatusCode_ReturnsFailureWithCustomStatus()
        {
            // Arrange
            var message = "Not found";

            // Act
            var result = Results.Failure(message, HttpStatusCode.NotFound);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal(message, result.Error.Message);
        }

        [Fact]
        public void Results_Failure_WithException_ReturnsFailureResultWithExceptionError()
        {
            // Arrange
            var exception = new InvalidOperationException("Invalid operation");

            // Act
            var result = Results.Failure(exception);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.IsType<ExceptionError>(result.Error);
            var exceptionError = (ExceptionError)result.Error;
            Assert.Equal(exception, exceptionError.Exception);
        }

        [Fact]
        public void Results_Failure_WithExceptionAndCustomStatusCode_ReturnsFailureWithCustomStatus()
        {
            // Arrange
            var exception = new ArgumentException("Argument invalid");

            // Act
            var result = Results.Failure(exception, HttpStatusCode.BadRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.IsType<ExceptionError>(result.Error);
        }

        [Fact]
        public void Results_Failure_DefaultStatusCodeIs422()
        {
            // Arrange
            var message = "Validation error";

            // Act
            var result = Results.Failure(message);

            // Assert
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
        }

        [Fact]
        public void Results_Success_WithNullData_WorksCorrectly()
        {
            // Act
            var result = Results.Success((object)null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.Data);
        }

        [Fact]
        public void Results_Success_WithEmptyMessage_WorksCorrectly()
        {
            // Act
            var result = Results.Success(string.Empty);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Results_Failure_WithEmptyMessage_WorksCorrectly()
        {
            // Act
            var result = Results.Failure(string.Empty);

            // Assert
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal(string.Empty, result.Error.Message);
        }

        [Fact]
        public void Results_Success_WithComplexData_WorksCorrectly()
        {
            // Arrange
            var data = new
            {
                Items = new[] { 1, 2, 3 },
                Total = 3,
                Status = "Active"
            };

            // Act
            var result = Results.Success(data, "Retrieved successfully");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(data, result.Data);
            Assert.Equal("Retrieved successfully", result.Message);
        }

        [Fact]
        public void Results_Failure_WithExceptionHavingInnerException_PreservesExceptionHierarchy()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner error");
            var exception = new Exception("Outer error", innerException);

            // Act
            var result = Results.Failure(exception);

            // Assert
            var exceptionError = (ExceptionError)result.Error;
            Assert.Equal(exception, exceptionError.Exception);
            Assert.Equal(innerException, exceptionError.Exception.InnerException);
        }
    }
}