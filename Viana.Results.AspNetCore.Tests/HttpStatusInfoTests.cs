using System.Net;

namespace Viana.Results.AspNetCore.Tests
{
    public class HttpStatusInfoTests
    {
        [Fact]
        public void GetMessage_WithHttpStatusCode_ReturnsCorrectMessage()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(HttpStatusCode.OK);

            // Assert
            Assert.Equal("OK", message);
        }

        [Fact]
        public void GetMessage_WithIntCode_ReturnsCorrectMessage()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(200);

            // Assert
            Assert.Equal("OK", message);
        }

        [Fact]
        public void GetMessage_WithNotFound_ReturnsCorrectMessage()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(HttpStatusCode.NotFound);

            // Assert
            Assert.Equal("Not Found", message);
        }

        [Fact]
        public void GetMessage_WithBadRequest_ReturnsCorrectMessage()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(HttpStatusCode.BadRequest);

            // Assert
            Assert.Equal("Bad Request", message);
        }

        [Fact]
        public void GetMessage_WithInternalServerError_ReturnsCorrectMessage()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(HttpStatusCode.InternalServerError);

            // Assert
            Assert.Equal("Internal Server Error", message);
        }

        [Fact]
        public void GetMessage_WithCreated_ReturnsCorrectMessage()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(HttpStatusCode.Created);

            // Assert
            Assert.Equal("Created", message);
        }

        [Fact]
        public void GetMessage_WithUnauthorized_ReturnsCorrectMessage()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(HttpStatusCode.Unauthorized);

            // Assert
            Assert.Equal("Unauthorized", message);
        }

        [Fact]
        public void GetMessage_WithForbidden_ReturnsCorrectMessage()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(HttpStatusCode.Forbidden);

            // Assert
            Assert.Equal("Forbidden", message);
        }

        [Fact]
        public void GetMessage_WithUnknownCode_ReturnsUnknow()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(999);

            // Assert
            Assert.Equal("Unknow", message);
        }

        [Fact]
        public void GetMessage_WithServiceUnavailable_ReturnsCorrectMessage()
        {
            // Act
            var message = HttpStatusInfo.GetMessage(HttpStatusCode.ServiceUnavailable);

            // Assert
            Assert.Equal("Service Unavailable", message);
        }

        [Theory]
        [InlineData(100, "Continue")]
        [InlineData(101, "Switching Protocols")]
        [InlineData(200, "OK")]
        [InlineData(201, "Created")]
        [InlineData(204, "No Content")]
        [InlineData(301, "Moved Permanently")]
        [InlineData(302, "Found")]
        [InlineData(400, "Bad Request")]
        [InlineData(401, "Unauthorized")]
        [InlineData(403, "Forbidden")]
        [InlineData(404, "Not Found")]
        [InlineData(500, "Internal Server Error")]
        [InlineData(502, "Bad Gateway")]
        [InlineData(503, "Service Unavailable")]
        [InlineData(422, "Unprocessable Content")]
        public void GetMessage_WithVariousCodes_ReturnsCorrectMessages(int code, string expectedMessage)
        {
            // Act
            var message = HttpStatusInfo.GetMessage(code);

            // Assert
            Assert.Equal(expectedMessage, message);
        }
    }
}