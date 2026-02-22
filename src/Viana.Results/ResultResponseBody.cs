namespace Viana.Results;

/// <summary>
/// Provides the same logic used to determine the HTTP response body for an <see cref="IResult"/>.
/// Used by MVC when writing the response and by OpenAPI/Swagger when generating response examples.
/// </summary>
public static class ResultResponseBody
{
	/// <summary>
	/// Returns the object that should be serialized as the response body for the given result,
	/// or null when there is no body (e.g. status 204 or non-generic result with no data).
	/// On error, returns <see cref="IResult.Problem"/>; on success with data, returns the unwrapped payload
	/// when the result is <see cref="IResultData"/> and the type is unwrappable and not scalar-like.
	/// </summary>
	/// <param name="result">The result to get the response body for.</param>
	/// <returns>The body to serialize, or null.</returns>
	public static object? GetBody(IResult? result)
	{
		if (result == null || result.Status == 204)
			return null;

		if (result.Problem != null)
			return result.Problem;

		var resultData = result as IResultData;
		var type = result.GetType();

		if (!type.IsGenericType)
			return null;

		if (!ResultReflections.IsUnwrappableType(type) || ResultReflections.IsScalarLike(type.GetGenericArguments()[0]))
			return resultData;

		return resultData?.Data ?? new object();
	}
}
