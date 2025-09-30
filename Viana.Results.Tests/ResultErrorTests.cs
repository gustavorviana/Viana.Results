namespace Viana.Results.Tests
{
    public class ResultErrorTests
    {
        [Fact]
        public void ResultError_Constructor_SetsMessage()
        {
            // Arrange
            var message = "An error occurred";

            // Act
            var error = new ResultError(message);

            // Assert
            Assert.Equal(message, error.Message);
        }

        [Fact]
        public void ResultError_WithEmptyMessage_SetsEmptyMessage()
        {
            // Arrange
            var message = string.Empty;

            // Act
            var error = new ResultError(message);

            // Assert
            Assert.Equal(message, error.Message);
        }

        [Fact]
        public void ResultError_WithNullMessage_SetsNull()
        {
            // Act
            var error = new ResultError(null);

            // Assert
            Assert.Null(error.Message);
        }
    }
}