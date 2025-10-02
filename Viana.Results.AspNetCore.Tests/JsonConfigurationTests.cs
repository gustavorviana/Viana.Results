using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Viana.Results.AspNetCore.Filters;

namespace Viana.Results.AspNetCore.Tests
{
    public class JsonConfigurationTests
    {
        [Fact]
        public void MvcOptions_ShouldConfigureFiltersAndJsonOptions()
        {
            // Arrange & Act
            var services = new ServiceCollection();
            services.AddControllers(options =>
            {
                options.Filters.Add<CustomResultFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            var serviceProvider = services.BuildServiceProvider();
            var mvcOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>();
            var jsonOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<JsonOptions>>();

            // Assert
            Assert.NotNull(mvcOptions);
            Assert.NotNull(jsonOptions);
            Assert.Equal(JsonNamingPolicy.CamelCase, jsonOptions.Value.JsonSerializerOptions.PropertyNamingPolicy);
            Assert.Contains(mvcOptions.Value.Filters, f =>
                f.GetType().Name == "TypeFilterAttribute" ||
                f.GetType().BaseType?.Name == "CustomResultFilter");
        }

        [Fact]
        public void JsonOptions_ShouldHaveCamelCaseNamingPolicy()
        {
            // Arrange & Act
            var services = new ServiceCollection();
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            var serviceProvider = services.BuildServiceProvider();
            var jsonOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<JsonOptions>>();

            // Assert
            Assert.NotNull(jsonOptions);
            Assert.Equal(JsonNamingPolicy.CamelCase, jsonOptions.Value.JsonSerializerOptions.PropertyNamingPolicy);
        }

        [Fact]
        public void MvcOptions_ShouldHaveCustomResultFilter()
        {
            // Arrange & Act
            var services = new ServiceCollection();
            services.AddControllers(options =>
            {
                options.Filters.Add<CustomResultFilter>();
            });

            var serviceProvider = services.BuildServiceProvider();
            var mvcOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>();

            // Assert
            Assert.NotNull(mvcOptions);
            Assert.Contains(mvcOptions.Value.Filters, f => f.GetType().Name == "TypeFilterAttribute");
        }

        [Fact]
        public void AddJsonOptions_WithCamelCase_SerializesCorrectly()
        {
            // Arrange
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var testObject = new TestPerson
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com"
            };

            // Act
            var json = JsonSerializer.Serialize(testObject, options);

            // Assert
            Assert.Contains("\"firstName\"", json);
            Assert.Contains("\"lastName\"", json);
            Assert.Contains("\"emailAddress\"", json);
            Assert.DoesNotContain("\"FirstName\"", json);
        }

        [Fact]
        public void AddJsonOptions_WithCamelCase_DeserializesCorrectly()
        {
            // Arrange
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = "{\"firstName\":\"John\",\"lastName\":\"Doe\",\"emailAddress\":\"john@example.com\"}";

            // Act
            var person = JsonSerializer.Deserialize<TestPerson>(json, options);

            // Assert
            Assert.NotNull(person);
            Assert.Equal("John", person.FirstName);
            Assert.Equal("Doe", person.LastName);
            Assert.Equal("john@example.com", person.EmailAddress);
        }

        [Fact]
        public void DefaultJsonSerializerOptions_ShouldHaveCamelCaseAsDefault()
        {
            // Arrange & Act
            var defaultOptions = ActionResultBase.DefaultJsonSerializerOptions;

            // Assert
            Assert.NotNull(defaultOptions);
            Assert.Equal(JsonNamingPolicy.CamelCase, defaultOptions.PropertyNamingPolicy);
        }

        [Fact]
        public void DefaultJsonSerializerOptions_CanBeCustomized()
        {
            // Arrange
            var originalOptions = ActionResultBase.DefaultJsonSerializerOptions;
            var customOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = true
            };

            try
            {
                // Act
                ActionResultBase.DefaultJsonSerializerOptions = customOptions;

                // Assert
                Assert.Equal(customOptions, ActionResultBase.DefaultJsonSerializerOptions);
                Assert.Null(ActionResultBase.DefaultJsonSerializerOptions.PropertyNamingPolicy);
                Assert.True(ActionResultBase.DefaultJsonSerializerOptions.WriteIndented);
            }
            finally
            {
                // Cleanup - restore original options
                ActionResultBase.DefaultJsonSerializerOptions = originalOptions;
            }
        }

        [Fact]
        public void DefaultJsonSerializerOptions_SerializesWithCamelCase()
        {
            // Arrange
            var testObject = new TestPerson
            {
                FirstName = "Jane",
                LastName = "Smith",
                EmailAddress = "jane@example.com"
            };

            // Act
            var json = JsonSerializer.Serialize(testObject, ActionResultBase.DefaultJsonSerializerOptions);

            // Assert
            Assert.Contains("\"firstName\"", json);
            Assert.Contains("\"lastName\"", json);
            Assert.Contains("\"emailAddress\"", json);
            Assert.DoesNotContain("\"FirstName\"", json);
            Assert.DoesNotContain("\"LastName\"", json);
            Assert.DoesNotContain("\"EmailAddress\"", json);
        }

        [Fact]
        public void MessageResult_WithCustomJsonOptions_SerializesErrorCorrectly()
        {
            // Arrange
            var customOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = true
            };

            var error = new ResultError("Validation failed");

            var messageResult = new MessageResult
            {
                Message = "Request failed",
                Error = error
            };

            // Act
            var returnObject = messageResult.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(messageResult, null);

            var json = JsonSerializer.Serialize(returnObject, customOptions);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\"message\"", json);
            Assert.Contains("\"error\"", json);
            Assert.Contains("Request failed", json);
            Assert.Contains("Validation failed", json);
        }

        [Fact]
        public void MessageResult_WithPascalCaseJsonOptions_SerializesErrorCorrectly()
        {
            // Arrange
            var pascalCaseOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // PascalCase by default
                WriteIndented = false
            };

            var error = new ResultError("Authentication failed");

            var messageResult = new MessageResult
            {
                Message = "Unauthorized access",
                Error = error
            };

            // Act
            var returnObject = messageResult.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(messageResult, null);

            var json = JsonSerializer.Serialize(returnObject, pascalCaseOptions);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\"Message\"", json);
            Assert.Contains("\"Error\"", json);
            Assert.Contains("Unauthorized access", json);
            Assert.Contains("Authentication failed", json);
        }

        [Fact]
        public void ObjectResult_WithDifferentJsonOptions_SerializesErrorData()
        {
            // Arrange
            var customOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var errorData = new
            {
                Field = "Email",
                Message = "Invalid email format",
                Code = "EMAIL_INVALID"
            };

            var objectResult = ObjectResult.ForError(System.Net.HttpStatusCode.BadRequest, errorData);

            // Act
            var returnObject = objectResult.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(objectResult, null);

            var json = JsonSerializer.Serialize(returnObject, customOptions);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\"field\"", json);
            Assert.Contains("\"message\"", json);
            Assert.Contains("\"code\"", json);
            Assert.Contains("Email", json);
            Assert.Contains("Invalid email format", json);
            Assert.Contains("EMAIL_INVALID", json);
        }

        [Fact]
        public void PageResult_WithCustomJsonOptions_SerializesWithError()
        {
            // Arrange
            var customOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var error = new ResultError("Database timeout");

            var pageResult = new PageResult
            {
                Items = new object[] { 1, 2, 3 },
                TotalItems = 100,
                Pages = 10,
                Error = error
            };

            // Act
            var returnObject = pageResult.GetType()
                .GetMethod("GetReturnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(pageResult, null);

            var json = JsonSerializer.Serialize(returnObject, customOptions);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\"data\"", json);
            Assert.Contains("\"totalItems\"", json);
            Assert.Contains("\"pages\"", json);
            Assert.Contains("\"error\"", json);
            Assert.Contains("Database timeout", json);
        }

        private class TestPerson
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EmailAddress { get; set; }
        }
    }
}
