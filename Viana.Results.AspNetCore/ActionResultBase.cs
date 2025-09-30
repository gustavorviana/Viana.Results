using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Viana.Results.AspNetCore
{
    /// <summary>
    /// Base class for custom action results that return JSON responses.
    /// </summary>
    public abstract class ActionResultBase(HttpStatusCode status) : ActionResult
    {
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
        protected abstract object GetReturnObject();

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

            var jsonOptions = GetJsonSerializerOptions(response);
            var jObj = JsonSerializer.SerializeToNode(GetReturnObject(), jsonOptions)!.AsObject();

            await response.WriteAsync(JsonSerializer.Serialize(jObj, jsonOptions));
        }

        private static JsonSerializerOptions GetJsonSerializerOptions(HttpResponse response)
        {
            return response.HttpContext.RequestServices
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
                    Message = result.Message,
                    Error = result.Error
                };

            if (result is IPaginatedResult paginated)
                return new PageResult
                {
                    Items = [.. (result.Data as IEnumerable)?.Cast<object>() ?? []],
                    Message = result.Message,
                    TotalItems = paginated.TotalCount,
                    Pages = paginated.Pages,
                    Error = result.Error
                };

            if (result is ICollectionResult)
                return new ItemsResult
                {
                    Items = ((IEnumerable)result.Data)?.Cast<object>().ToList(),
                    Message = result.Message,
                    Error = result.Error
                };

            return new ObjectResult(result.StatusCode)
            {
                Data = result.Data!,
                Message = result.Message,
                Error = result.Error
            };
        }
    }
}
