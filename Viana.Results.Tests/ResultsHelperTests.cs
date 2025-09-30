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
            var result = Results.Success(message, data);

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
            var result = Results.Success("Retrieved successfully", data);

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

        [Fact]
        public void Results_BusinessRuleViolated_WithMessage_ReturnsBusinessRuleViolatedResult()
        {
            // Arrange
            var message = "Cannot delete an active user";

            // Act
            var result = Results.BusinessRuleViolated(message);

            // Assert
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal(message, result.Error.Message);
            Assert.Equal(message, result.Message);
            Assert.IsType<ResultError>(result.Error);
        }

        [Fact]
        public void Results_BusinessRuleViolated_WithMessageAndData_ReturnsDataResultError()
        {
            // Arrange
            var message = "Invalid operation";
            var data = new { Reason = "User is locked", UserId = 123 };

            // Act
            var result = Results.BusinessRuleViolated(message, data);

            // Assert
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.IsType<DataResultError>(result.Error);
            Assert.Equal(message, result.Error.Message);
            Assert.Equal(message, result.Message);
            var dataError = (DataResultError)result.Error;
            Assert.Equal(data, dataError.Data);
        }

        [Fact]
        public void Results_BusinessRuleViolated_WithNullData_ReturnsResultError()
        {
            // Arrange
            var message = "Business rule violation";

            // Act
            var result = Results.BusinessRuleViolated(message, null);

            // Assert
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.IsType<ResultError>(result.Error);
            Assert.Equal(message, result.Error.Message);
        }

        [Fact]
        public void Results_NotFound_WithDefaultMessage_ReturnsNotFoundResult()
        {
            // Act
            var result = Results.NotFound();

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal("The requested resource was not found.", result.Error.Message);
            Assert.Equal("The requested resource was not found.", result.Message);
        }

        [Fact]
        public void Results_NotFound_WithCustomMessage_ReturnsNotFoundResultWithCustomMessage()
        {
            // Arrange
            var message = "User not found";

            // Act
            var result = Results.NotFound(message);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal(message, result.Error.Message);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void Results_Unauthorized_WithDefaultMessage_ReturnsUnauthorizedResult()
        {
            // Act
            var result = Results.Unauthorized();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal("Unauthorized access.", result.Error.Message);
            Assert.Equal("Unauthorized access.", result.Message);
        }

        [Fact]
        public void Results_Unauthorized_WithCustomMessage_ReturnsUnauthorizedResultWithCustomMessage()
        {
            // Arrange
            var message = "Invalid credentials";

            // Act
            var result = Results.Unauthorized(message);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal(message, result.Error.Message);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void Results_Forbidden_WithDefaultMessage_ReturnsForbiddenResult()
        {
            // Act
            var result = Results.Forbidden();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal("Forbidden access.", result.Error.Message);
            Assert.Equal("Forbidden access.", result.Message);
        }

        [Fact]
        public void Results_Forbidden_WithCustomMessage_ReturnsForbiddenResultWithCustomMessage()
        {
            // Arrange
            var message = "You do not have permission to access this resource";

            // Act
            var result = Results.Forbidden(message);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal(message, result.Error.Message);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void Results_Conflict_WithDefaultMessage_ReturnsConflictResult()
        {
            // Act
            var result = Results.Conflict();

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal("Conflict occurred.", result.Error.Message);
            Assert.Equal("Conflict occurred.", result.Message);
        }

        [Fact]
        public void Results_Conflict_WithCustomMessage_ReturnsConflictResultWithCustomMessage()
        {
            // Arrange
            var message = "Email already exists";

            // Act
            var result = Results.Conflict(message);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.Equal(message, result.Error.Message);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void Results_Validation_WithErrors_ReturnsValidationResult()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required", "Email is invalid" } },
                { "Password", new[] { "Password is too short" } }
            };

            // Act
            var result = Results.Validation(errors);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.IsType<ValidationError>(result.Error);
            Assert.Equal("Validation failed", result.Error.Message);
            var validationError = (ValidationError)result.Error;
            Assert.Equal(errors, validationError.Errors);
        }

        [Fact]
        public void Results_Validation_WithErrorsAndCustomMessage_ReturnsValidationResultWithCustomMessage()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Username", new[] { "Username is required" } }
            };
            var message = "Form validation failed";

            // Act
            var result = Results.Validation(errors, message);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.IsType<ValidationError>(result.Error);
            Assert.Equal(message, result.Error.Message);
            var validationError = (ValidationError)result.Error;
            Assert.Equal(errors, validationError.Errors);
        }

        [Fact]
        public void Results_Validation_WithEmptyErrors_ReturnsValidationResult()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>();

            // Act
            var result = Results.Validation(errors);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.IsType<ValidationError>(result.Error);
            var validationError = (ValidationError)result.Error;
            Assert.Empty(validationError.Errors);
        }

        [Fact]
        public void Results_Validation_WithNullErrors_WorksCorrectly()
        {
            // Act
            var result = Results.Validation((Dictionary<string, string[]>)null);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Assert.NotNull(result.Error);
            Assert.IsType<ValidationError>(result.Error);
            var validationError = (ValidationError)result.Error;
            Assert.NotNull(validationError.Errors);
            Assert.Empty(validationError.Errors);
        }
    }
}