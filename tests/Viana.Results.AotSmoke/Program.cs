using System.Text.Json;
using Viana.Results;
using Viana.Results.AotSmoke;

// Exercises the core paths an AOT app uses: problem JSON (converter read and write, with object extensions),
// results, lists, pages and envelope bodies. Exits non-zero on any mismatch.
var options = new JsonSerializerOptions { TypeInfoResolver = SmokeJsonContext.Default, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var failures = 0;

void Check(bool ok, string what)
{
    if (!ok)
    {
        failures++;
        Console.Error.WriteLine($"FAIL: {what}");
    }
}

var problem = new ProblemBuilder(404)
    .WithTitle("Missing")
    .WithDetail("No item 7.")
    .AddExtension("traceId", "abc")
    .AddExtension("item", new Dictionary<string, object?> { ["id"] = 7 })
    .Build();
var problemInfo = (System.Text.Json.Serialization.Metadata.JsonTypeInfo<ProblemResult>)options.GetTypeInfo(typeof(ProblemResult));
var problemJson = JsonSerializer.Serialize(problem, problemInfo);
var problemBack = JsonSerializer.Deserialize(problemJson, problemInfo)!;
Check(problemBack.Status == 404 && problemBack.Title == "Missing", "problem round trip");
Check(problemJson.Contains("\"traceId\":\"abc\""), "problem extension written");

Result<SmokeItem> typed = new SmokeItem { Id = 1, Name = "one" };
Check(ResultResponseBody.GetBody(typed, options) is SmokeItem { Id: 1 }, "typed result unwrapped");
Check(ResultResponseBody.GetBody(Results.Ok(), options) == null, "bare result has no body");

var paged = PagedResult<SmokeItem>.Create([new SmokeItem { Id = 2 }], 1, 10, 1);
var pagedBody = ResultResponseBody.GetBody(paged, options)!;
var pagedJson = JsonSerializer.Serialize(pagedBody, options.GetTypeInfo(pagedBody.GetType()));
Check(pagedJson.Contains("\"pageNumber\":1") && !pagedJson.Contains("\"status\""), "paged envelope");

var cursor = new SmokeCursorResult([new SmokeItem { Id = 3 }], "next");
var cursorBody = ResultResponseBody.GetBody(cursor, options)!;
var cursorJson = JsonSerializer.Serialize(cursorBody, options.GetTypeInfo(cursorBody.GetType()));
Check(cursorJson.Contains("\"nextCursor\":\"next\"") && !cursorJson.Contains("\"status\""), "shaped envelope");

Console.WriteLine(failures == 0 ? "AOT smoke: ok" : $"AOT smoke: {failures} failure(s)");
return failures == 0 ? 0 : 1;
