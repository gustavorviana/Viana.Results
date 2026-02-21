using System.Net;
using System.Net.Sockets;

namespace Viana.Results.Tests
{
    public class ResultsHelperTests
    {
        [Fact]
        public void Results_Ok_WithMessage_ReturnsSuccessResult()
        {
            // Arrange
            var message = "Operation completed successfully";

            // Act
            var result = Results.Ok(message);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Equal(message, result.Data);
            Assert.Null(result.Problem);
        }

        [Fact]
        public void Results_Ok_WithData_ReturnsSuccessResultWithData()
        {
            // Arrange
            var data = new { Id = 1, Name = "Test" };

            // Act
            var result = Results.Ok(data);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Equal(data, result.Data);
            Assert.Null(result.Problem);
        }

        [Fact]
        public void Results_Ok_WithDataAndMessage_ReturnsSuccessResultWithBoth()
        {
            // Arrange
            var data = "test data";

            // Act
            var result = Results.Ok(data);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Equal(data, result.Data);
            Assert.Null(result.Problem);
        }

        [Fact]
        public void Results_Ok_WithoutParameters_ReturnsSuccessResult()
        {
            // Act
            var result = Results.Ok();

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Null(result.Problem);
        }

        [Fact]
        public void Results_Failure_WithMessage_ReturnsFailureResult()
        {
            // Arrange
            var message = "Operation failed";

            // Act
            var result = Results.Failure(HttpStatusCode.InternalServerError, message);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal(message, result.Problem.Title);
        }

        [Fact]
        public void Results_Failure_WithMessageAndCustomStatusCode_ReturnsFailureWithCustomStatus()
        {
            // Arrange
            var message = "Not found";

            // Act
            var result = Results.Failure(HttpStatusCode.NotFound, message);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal(message, result.Problem.Title);
        }

        [Fact]
        public void Results_Failure_WithException_ReturnsFailureResultWithProblemResult()
        {
            // Arrange
            var exception = new InvalidOperationException("Invalid operation");

            // Act
            var result = Results.Failure(exception);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal(exception.Message, result.Problem!.Title);
        }

        [Fact]
        public void Results_Failure_WithExceptionAndCustomStatusCode_ReturnsFailureWithCustomStatus()
        {
            // Arrange
            var exception = new ArgumentException("Argument invalid");

            // Act
            var result = Results.Failure(exception, HttpStatusCode.BadRequest);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal(exception.Message, result.Problem!.Title);
        }

        [Fact]
        public void Results_Failure_DefaultStatusCodeIs422()
        {
            // Arrange
            var message = "Validation error";

            // Act
            var result = Results.Failure(HttpStatusCode.InternalServerError, message);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
        }

        [Fact]
        public void Results_Ok_WithNullData_WorksCorrectly()
        {
            // Act
            var result = Results.Ok((object)null);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Null(result.Data);
        }

        [Fact]
        public void Results_Ok_WithEmptyMessage_WorksCorrectly()
        {
            // Act
            var result = Results.Ok(string.Empty);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Equal(string.Empty, result.Data);
        }

        [Fact]
        public void Results_Failure_WithEmptyMessage_WorksCorrectly()
        {
            // Act
            var result = Results.Failure(HttpStatusCode.InternalServerError, string.Empty);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal("Internal Server Error", result.Problem.Title);
        }

        [Fact]
        public void Results_Ok_WithComplexData_WorksCorrectly()
        {
            // Arrange
            var data = new
            {
                Items = new[] { 1, 2, 3 },
                Total = 3,
                Status = "Active"
            };

            // Act
            var result = Results.Ok(data);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void Results_Failure_WithExceptionHavingInnerException_UsesOuterMessageAsTitle()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner error");
            var exception = new Exception("Outer error", innerException);

            // Act
            var result = Results.Failure(exception);

            // Assert
            Assert.NotNull(result.Problem);
            Assert.Equal("Outer error", result.Problem!.Title);
        }

        [Fact]
        public void Results_BusinessRuleViolated_WithMessage_ReturnsDataProblemResult()
        {
            // Act
            var result = Results.BusinessRuleViolated("User is locked");

            // Assert
            Assert.Equal(422, result.Status);
            Assert.NotNull(result.Problem);
            Assert.Equal("User is locked", result.Problem.Title);
        }

        [Fact]
        public void Results_NotFound_WithDefaultMessage_ReturnsNotFoundResult()
        {
            // Act
            var result = Results.NotFound();

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal("The requested resource was not found.", result.Problem.Title);
        }

        [Fact]
        public void Results_NotFound_WithCustomMessage_ReturnsNotFoundResultWithCustomMessage()
        {
            // Arrange
            var message = "User not found";

            // Act
            var result = Results.NotFound(message);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal(message, result.Problem.Title);
        }

        [Fact]
        public void Results_Unauthorized_WithDefaultMessage_ReturnsUnauthorizedResult()
        {
            // Act
            var result = Results.Unauthorized();

            // Assert
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal("Unauthorized access.", result.Problem.Title);
        }

        [Fact]
        public void Results_Unauthorized_WithCustomMessage_ReturnsUnauthorizedResultWithCustomMessage()
        {
            // Arrange
            var message = "Invalid credentials";

            // Act
            var result = Results.Unauthorized(message);

            // Assert
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal(message, result.Problem.Title);
        }

        [Fact]
        public void Results_Forbidden_WithDefaultMessage_ReturnsForbiddenResult()
        {
            // Act
            var result = Results.Forbidden();

            // Assert
            Assert.Equal((int)HttpStatusCode.Forbidden, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal("Forbidden access.", result.Problem.Title);
        }

        [Fact]
        public void Results_Forbidden_WithCustomMessage_ReturnsForbiddenResultWithCustomMessage()
        {
            // Arrange
            var message = "You do not have permission to access this resource";

            // Act
            var result = Results.Forbidden(message);

            // Assert
            Assert.Equal((int)HttpStatusCode.Forbidden, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal(message, result.Problem.Title);
        }

        [Fact]
        public void Results_Conflict_WithDefaultMessage_ReturnsConflictResult()
        {
            // Act
            var result = Results.Conflict();

            // Assert
            Assert.Equal((int)HttpStatusCode.Conflict, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal("Conflict occurred.", result.Problem.Title);
        }

        [Fact]
        public void Results_Conflict_WithCustomMessage_ReturnsConflictResultWithCustomMessage()
        {
            // Arrange
            var message = "Email already exists";

            // Act
            var result = Results.Conflict(message);

            // Assert
            Assert.Equal((int)HttpStatusCode.Conflict, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal(message, result.Problem.Title);
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
            Assert.Equal((int)HttpStatusCode.BadRequest, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.True(result.Problem!.Extensions.TryGetValue("errors", out var ext) && ext is Dictionary<string, string[]> dict && dict.Count == 2);
        }

        [Fact]
        public void Results_Validation_WithErrorsAndCustomMessage_ReturnsValidationResultWithCustomMessage()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Username", new[] { "Username is required" } }
            };

            // Act
            var result = Results.Validation(errors, "Custom validation failed");

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
            Assert.Equal("Custom validation failed", result.Problem!.Title);
        }

        [Fact]
        public void Results_Validation_WithEmptyErrors_ReturnsValidationResult()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>();

            // Act
            var result = Results.Validation(errors);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
        }

        [Fact]
        public void Results_Validation_WithNullErrors_WorksCorrectly()
        {
            // Act
            var result = Results.Validation((Dictionary<string, string[]>)null!);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, result.Status);
            Assert.NotNull(result.Problem);
            Assert.IsType<ProblemResult>(result.Problem);
        }
    }
}