namespace Viana.Results.Tests
{
    public class DataResultErrorTests
    {
        [Fact]
        public void DataResultError_Constructor_SetsDataAndMessage()
        {
            // Arrange
            var data = new { Field = "Invalid", Value = "123" };
            var message = "Validation error";

            // Act
            var error = new DataResultError(data, message);

            // Assert
            Assert.Equal(data, error.Data);
            Assert.Equal(message, error.Message);
        }

        [Fact]
        public void DataResultError_WithoutMessage_SetsDefaultMessage()
        {
            // Arrange
            var data = new { Field = "Invalid" };

            // Act
            var error = new DataResultError(data);

            // Assert
            Assert.Equal(data, error.Data);
            Assert.Null(error.Message);
        }

        [Fact]
        public void DataResultError_WithNullMessage_SetsDefaultMessage()
        {
            // Arrange
            var data = "test data";

            // Act
            var error = new DataResultError(data, null);

            // Assert
            Assert.Equal(data, error.Data);
            Assert.Null(error.Message);
        }

        [Fact]
        public void DataResultError_InheritsFromResultError()
        {
            // Arrange
            var data = "test";

            // Act
            var error = new DataResultError(data);

            // Assert
            Assert.IsAssignableFrom<ResultError>(error);
        }

        [Fact]
        public void DataResultError_WithComplexData_WorksCorrectly()
        {
            // Arrange
            var data = new
            {
                Errors = new[] { "Error 1", "Error 2" },
                Code = 400
            };

            // Act
            var error = new DataResultError(data, "Validation failed");

            // Assert
            Assert.Equal(data, error.Data);
            Assert.Equal("Validation failed", error.Message);
        }
    }
}
