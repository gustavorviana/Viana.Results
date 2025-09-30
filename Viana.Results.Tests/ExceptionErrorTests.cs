namespace Viana.Results.Tests
{
    public class ExceptionErrorTests
    {
        [Fact]
        public void ExceptionError_Constructor_WithException_SetsExceptionAndMessage()
        {
            // Arrange
            var exception = new InvalidOperationException("Invalid operation");

            // Act
            var error = new ExceptionError(exception);

            // Assert
            Assert.Equal(exception, error.Exception);
            Assert.Equal("Invalid operation", error.Message);
        }

        [Fact]
        public void ExceptionError_Constructor_WithExceptionAndMessage_SetsProperties()
        {
            // Arrange
            var exception = new ArgumentException("Argument invalid");
            var message = "Custom error message";

            // Act
            var error = new ExceptionError(exception, message);

            // Assert
            Assert.Equal(exception, error.Exception);
            Assert.Equal(message, error.Message);
        }

        [Fact]
        public void ExceptionError_InheritsFromResultError()
        {
            // Arrange
            var exception = new Exception("Test");

            // Act
            var error = new ExceptionError(exception);

            // Assert
            Assert.IsAssignableFrom<ResultError>(error);
        }

        [Fact]
        public void ExceptionError_WithDifferentExceptionTypes_WorksCorrectly()
        {
            // Arrange & Act
            var divideByZeroError = new ExceptionError(new DivideByZeroException("Cannot divide by zero"));
            var nullRefError = new ExceptionError(new NullReferenceException("Object is null"));
            var argumentError = new ExceptionError(new ArgumentNullException("parameter"));

            // Assert
            Assert.IsType<DivideByZeroException>(divideByZeroError.Exception);
            Assert.IsType<NullReferenceException>(nullRefError.Exception);
            Assert.IsType<ArgumentNullException>(argumentError.Exception);
        }

        [Fact]
        public void ExceptionError_PreservesExceptionStackTrace()
        {
            // Arrange
            Exception exception;
            try
            {
                throw new InvalidOperationException("Test exception");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Act
            var error = new ExceptionError(exception);

            // Assert
            Assert.Equal(exception, error.Exception);
            Assert.NotNull(error.Exception.StackTrace);
        }

        [Fact]
        public void ExceptionError_WithInnerException_PreservesInnerException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner error");
            var exception = new Exception("Outer error", innerException);

            // Act
            var error = new ExceptionError(exception);

            // Assert
            Assert.Equal(exception, error.Exception);
            Assert.Equal(innerException, error.Exception.InnerException);
        }
    }
}
