using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Viana.Results.Mvc;

/// <summary>
/// Action result that writes an <see cref="IResult"/> as JSON: sets the HTTP status from <see cref="IResult.Status"/>,
/// and writes a response body when available—on error, RFC 9457 problem details (<see cref="IResult.Problem"/>);
/// on success, the payload from <see cref="IResultData.Data"/> when the result implements <see cref="IResultData"/>.
/// </summary>
/// <param name="result">The result to serialize to the HTTP response.</param>
public class VianaResultAction(IResult result) : ActionResult
{
    /// <summary>
    /// Gets the collection of HTTP headers to include in the response (e.g. custom headers).
    /// The "content-type" header is set automatically and is not copied from this collection.
    /// </summary>
    public HeaderDictionary Headers { get; } = [];

    /// <summary>
    /// Gets the value used for the response "Content-Type" header. Default is "application/json".
    /// </summary>
    protected virtual string ContentType { get; } = "application/json";

    /// <summary>
    /// Executes the result asynchronously: sets status code and headers, then writes the JSON body
    /// (problem details on error, or payload on success when the result exposes data).
    /// </summary>
    /// <param name="context">The action context providing the HTTP response.</param>
    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = result.Status;
        WriteHeaders(response);

        var body = GetExpectedBody();
        if (body == null)
            return;

        response.ContentType = ContentType;
        await response.WriteAsync(SerializeToJson(response, body));
    }

    private object? GetExpectedBody()
    {
        if (result == null) 
            return null;

        if (result.Problem != null)
            return result.Problem;

        if (IsRawResult())
            return (result as IResultData)?.Data;

        return result;
    }

    private bool IsRawResult()
    {
        var type = result.GetType();
        if (type == typeof(Result))
            return true;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
            return true;

        return false;
    }

    /// <summary>
    /// Writes <see cref="Headers"/> to the response, excluding "content-type" (case-insensitive),
    /// which is set separately from <see cref="ContentType"/>.
    /// </summary>
    /// <param name="response">The HTTP response to write headers to.</param>
    protected virtual void WriteHeaders(HttpResponse response)
    {
        foreach (var header in Headers)
            if (!header.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                response.Headers[header.Key] = header.Value;
    }

    /// <summary>
    /// Serializes the given value to JSON using the serializer options from DI, <see cref="DefaultJsonSerializerOptions"/>,
    /// or a new <see cref="JsonSerializerOptions"/> instance as fallback.
    /// </summary>
    /// <param name="response">The HTTP response; used to resolve <see cref="JsonOptions"/> from request services.</param>
    /// <param name="value">The object to serialize (e.g. problem details or payload). May be null.</param>
    /// <returns>The JSON string to write to the response body.</returns>
    protected virtual string SerializeToJson(HttpResponse response, object? value)
    {
        var jsonOptions = GetJsonSerializerOptions(response);
        return JsonSerializer.Serialize(value, jsonOptions);
    }

    private static JsonSerializerOptions GetJsonSerializerOptions(HttpResponse response)
    {
        return response.HttpContext.RequestServices
            .GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions
            ?? new JsonSerializerOptions();
    }
}