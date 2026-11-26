using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Viana.Results.AotSmoke;

/// <summary>Source-generated metadata: the only JSON contracts available in the AOT app.</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ProblemResult))]
[JsonSerializable(typeof(SmokeItem))]
[JsonSerializable(typeof(IReadOnlyList<SmokeItem>))]
[JsonSerializable(typeof(PagedResult<SmokeItem>))]
[JsonSerializable(typeof(SmokeCursorResult))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
public partial class SmokeJsonContext : JsonSerializerContext
{
}
