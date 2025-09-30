namespace Viana.Results.Tests
{
    public class ValidationErrorTests
    {
        [Fact]
        public void ValidationError_Constructor_WithDictionary_SetsErrorsAndMessage()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required", "Email is invalid" } },
                { "Password", new[] { "Password is too short" } }
            };

            // Act
            var error = new ValidationError(errors);

            // Assert
            Assert.Equal(errors, error.Errors);
        }

        [Fact]
        public void ValidationError_Constructor_WithDictionary_WithoutMessage_SetsDefaultMessage()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Username", new[] { "Username is required" } }
            };

            // Act
            var error = new ValidationError(errors);

            // Assert
            Assert.Equal(errors, error.Errors);
            Assert.Null(error.Message);
        }

        [Fact]
        public void ValidationError_Constructor_WithNullDictionary_InitializesEmptyDictionary()
        {
            // Act
            var error = new ValidationError((Dictionary<string, string[]>)null);

            // Assert
            Assert.NotNull(error.Errors);
            Assert.Empty(error.Errors);
            Assert.Null(error.Message);
        }

        [Fact]
        public void ValidationError_Constructor_WithSingleField_CreatesErrorDictionary()
        {
            // Arrange
            var field = "Email";
            var messages = new[] { "Email is required", "Email format is invalid" };

            // Act
            var error = new ValidationError(field, messages);

            // Assert
            Assert.Single(error.Errors);
            Assert.True(error.Errors.ContainsKey(field));
            Assert.Equal(messages, error.Errors[field]);
            Assert.Equal("Validation failed", error.Message);
        }

        [Fact]
        public void ValidationError_Constructor_WithSingleField_SingleMessage_WorksCorrectly()
        {
            // Arrange
            var field = "Age";
            var message = "Age must be positive";

            // Act
            var error = new ValidationError(field, message);

            // Assert
            Assert.Single(error.Errors);
            Assert.True(error.Errors.ContainsKey(field));
            Assert.Single(error.Errors[field]);
            Assert.Equal(message, error.Errors[field][0]);
        }

        [Fact]
        public void ValidationError_Constructor_WithSingleField_NullMessages_InitializesEmptyArray()
        {
            // Arrange
            var field = "Name";

            // Act
            var error = new ValidationError(field, null);

            // Assert
            Assert.Single(error.Errors);
            Assert.True(error.Errors.ContainsKey(field));
            Assert.Empty(error.Errors[field]);
        }

        [Fact]
        public void ValidationError_InheritsFromResultError()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Field", new[] { "Error" } }
            };

            // Act
            var error = new ValidationError(errors);

            // Assert
            Assert.IsAssignableFrom<ResultError>(error);
        }

        [Fact]
        public void ValidationError_WithMultipleFields_WorksCorrectly()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "FirstName", new[] { "First name is required" } },
                { "LastName", new[] { "Last name is required" } },
                { "Email", new[] { "Email is required", "Email format is invalid" } },
                { "Phone", new[] { "Phone number is invalid" } }
            };

            // Act
            var error = new ValidationError(errors);

            // Assert
            Assert.Equal(4, error.Errors.Count);
            Assert.Null(error.Message);
            Assert.Equal(new[] { "First name is required" }, error.Errors["FirstName"]);
            Assert.Equal(new[] { "Last name is required" }, error.Errors["LastName"]);
            Assert.Equal(new[] { "Email is required", "Email format is invalid" }, error.Errors["Email"]);
            Assert.Equal(new[] { "Phone number is invalid" }, error.Errors["Phone"]);
        }

        [Fact]
        public void ValidationError_WithEmptyDictionary_WorksCorrectly()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>();

            // Act
            var error = new ValidationError(errors);

            // Assert
            Assert.Empty(error.Errors);
            Assert.Null(error.Message);
        }

        [Fact]
        public void ValidationError_WithSingleField_NoMessages_WorksCorrectly()
        {
            // Arrange
            var field = "Status";

            // Act
            var error = new ValidationError(field);

            // Assert
            Assert.Single(error.Errors);
            Assert.True(error.Errors.ContainsKey(field));
            Assert.Empty(error.Errors[field]);
        }
    }
}
