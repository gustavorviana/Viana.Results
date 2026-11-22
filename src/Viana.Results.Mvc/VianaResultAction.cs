using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        if (!ResultResponseBody.HasBody(result))
            return;

        var body = ResultResponseBody.GetBody(result);
        response.ContentType = result.Problem != null ? ProblemContentType : ContentType;

        var cancellationToken = context.HttpContext.RequestAborted;
        if (cancellationToken.IsCancellationRequested)
            return;

        try
        {
            if (OverridesSerializeToJson(GetType()))
            {
                await response.WriteAsync(SerializeToJson(response, body), cancellationToken);
                return;
            }

            await JsonSerializer.SerializeAsync(response.Body, body, body?.GetType() ?? typeof(object), GetJsonSerializerOptions(response), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The client went away: stop writing silently.
        }
    }

    /// <summary>RFC 9457 media type used for problem responses.</summary>
    private const string ProblemContentType = "application/problem+json";

    private static readonly ConcurrentDictionary<Type, bool> SerializeOverrides = new();

    // A subclass that overrides SerializeToJson keeps its custom output (legacy string path).
    private static bool OverridesSerializeToJson(Type type) => SerializeOverrides.GetOrAdd(type, static t =>
        t.GetMethod(nameof(SerializeToJson), BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(HttpResponse), typeof(object) }, null)?.DeclaringType != typeof(VianaResultAction));

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
    /// Serializes the given value to JSON using the <see cref="JsonOptions"/> configured in the
    /// request services when available; otherwise falls back to a new <see cref="JsonSerializerOptions"/>
    /// instance with default settings.
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
        var options = response.HttpContext.RequestServices
            .GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions
            ?? DefaultOptions;
        return LegacyNameOptions.GetValue(options, CreateLegacyNameOptions);
    }

    private static readonly JsonSerializerOptions DefaultOptions = new();

    // The old MVC package keeps extension names as stored (original rule), even when the core
    // converter applies the naming policy. Options converters win over the [JsonConverter] on the type.
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> LegacyNameOptions = new();

    private static JsonSerializerOptions CreateLegacyNameOptions(JsonSerializerOptions source)
    {
        var copy = new JsonSerializerOptions(source);
        copy.Converters.Insert(0, new ProblemResultJsonConverter { PreserveExtensionNames = true });
        return copy;
    }
}