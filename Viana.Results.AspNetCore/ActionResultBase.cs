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
    public abstract class ActionResultBase(HttpStatusCode status) : ActionResult
    {
        public int? StatusCode { get; } = (int)status;
        public ResultError Error { get; set; }
        public HeaderDictionary Headers { get; } = [];

        protected abstract object GetReturnObject();

        public override Task ExecuteResultAsync(ActionContext context)
        {
            return WriteToAsync(context.HttpContext.Response);
        }

        public async Task WriteToAsync(HttpResponse response)
        {
            response.StatusCode = StatusCode ?? 500;
            response.ContentType = "application/json";

            foreach (var header in Headers)
                if (header.Key.ToLower() != "content-type")
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

        public static ActionResultBase FromResult(IResult result)
        {
            var type = result.GetType();
            if (!type.IsGenericType)
                return new MessageResult(result.StatusCode)
                {
                    Message = result.Message,
                    Error = result.Error
                };

            var dataProperty = result.GetType().GetProperty("Data");
            var value = dataProperty?.GetValue(result);

            if (result is IPaginatedResult paginated)
                return new PageResult
                {
                    Items = [.. (value as IEnumerable)?.Cast<object>() ?? []],
                    Message = result.Message,
                    TotalItems = paginated.TotalCount,
                    Pages = paginated.Pages,
                    Error = result.Error
                };

            if (result is ICollectionResult)
                return new ItemsResult
                {
                    Items = ((IEnumerable)value)?.Cast<object>().ToList(),
                    Message = result.Message,
                    Error = result.Error
                };

            return new ObjectResult(result.StatusCode)
            {
                Result = value!,
                Message = result.Message,
                Error = result.Error
            };
        }
    }
}
