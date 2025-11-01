using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Viana.Results.AspNetCore
{
    /// <summary>
    /// Base class for custom action results that return JSON responses.
    /// </summary>
    public abstract class ActionResultBase(HttpStatusCode status) : ActionResult
    {
        /// <summary>
        /// Gets or sets the default JSON serializer options to use when no options are configured in the DI container.
        /// This provides a hardcoded fallback configuration.
        /// </summary>
        public static JsonSerializerOptions DefaultJsonSerializerOptions { get; set; }

        /// <summary>
        /// Gets the HTTP status code for the response.
        /// </summary>
        public int? StatusCode { get; } = (int)status;

        /// <summary>
        /// Gets or sets the error information for the response.
        /// </summary>
        public ResultError Error { get; set; }

        /// <summary>
        /// Gets the collection of HTTP headers to include in the response.
        /// </summary>
        public HeaderDictionary Headers { get; } = [];

        /// <summary>
        /// Gets the object to be serialized and returned in the response.
        /// </summary>
        /// <returns>The object to serialize.</returns>
        protected abstract object GetReturnObject(ResponseFormatOptions options);

        public override Task ExecuteResultAsync(ActionContext context)
        {
            return WriteToAsync(context.HttpContext.Response);
        }

        public async Task WriteToAsync(HttpResponse response)
        {
            response.StatusCode = StatusCode ?? 200;
            response.ContentType = "application/json";

            foreach (var header in Headers)
                if (!header.Key.Equals("content-type", System.StringComparison.CurrentCultureIgnoreCase))
                    response.Headers[header.Key] = header.Value;

            var returnObject = GetReturnObject(GetOptions(response.HttpContext));
            if (returnObject == null)
                return;

            var jsonOptions = GetJsonSerializerOptions(response);
            await response.WriteAsync(JsonSerializer.Serialize(returnObject, jsonOptions));
        }

        private static JsonSerializerOptions GetJsonSerializerOptions(HttpResponse response)
        {
            return DefaultJsonSerializerOptions ?? response.HttpContext.RequestServices
                .GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions
                ?? new JsonSerializerOptions();
        }

        /// <summary>
        /// Converts an IResult instance to an appropriate ActionResultBase implementation.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        /// <returns>An ActionResultBase instance representing the result.</returns>
        public static ActionResultBase FromResult(IResult result)
        {
            if (result.Data == null)
                return new MessageResult(result.StatusCode)
                {
                    Message = result.Data?.ToString(),
                    Error = result.Error
                };

            if (result is IPaginatedResult paginated)
                return new PageResult
                {
                    Items = [.. (result.Data as IEnumerable)?.Cast<object>() ?? []],
                    Total = paginated.Total,
                    Pages = paginated.Pages,
                    Error = result.Error
                };

            if (result is ICollectionResult)
                return new ItemsResult
                {
                    Items = ((IEnumerable)result.Data)?.Cast<object>().ToList(),
                    Error = result.Error
                };

            return new ObjectResult(result.StatusCode)
            {
                Data = result.Data!,
                Error = result.Error
            };
        }

        protected ResponseFormatOptions GetOptions(HttpContext context)
        {
            return context
                .RequestServices
                .GetRequiredService<IOptions<ResponseFormatOptions>>()
                ?.Value ?? new ResponseFormatOptions();
        }

        public class DataResponse
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Message { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public object Error { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public object Data { get; set; }
        }
    }
}
