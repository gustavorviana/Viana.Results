using System;

namespace Viana.Results.OpenApi;

/// <summary>
/// Removes the success response that the documentation infers from the return type (for a bare
/// <see cref="Result"/> that is <c>204 No Content</c>; for <c>Result&lt;T&gt;</c> the <c>200</c> with <c>T</c>).
/// Only the responses registered by hand (<c>[ProducesResponseType]</c>, <see cref="ResponseExampleAttribute"/>,
/// <c>ProducesResult</c>, <c>Produces</c>) and the problem responses are documented.
/// </summary>
/// <remarks>
/// A last resort, for endpoints that return the base <see cref="Result"/> but can answer different successes
/// (for example a different body per kind of client, or 200 or 202 depending on the request). It changes the
/// documentation only: the runtime response is the result itself. On a controller class it applies to every
/// action. On minimal APIs use <c>WithoutDefaultResult()</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class WithoutDefaultResultAttribute : Attribute
{
}
