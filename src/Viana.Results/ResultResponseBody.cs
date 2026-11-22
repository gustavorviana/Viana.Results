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
		if (result == null)
			return null;

		// A problem is always written, whatever the status (a 204 must not drop it).
		if (result.Problem != null)
			return result.Problem;

		if (result.Status == 204)
			return null;

		var resultData = result as IResultData;
		var type = result.GetType();

		if (!type.IsGenericType)
			return null;

		if (!ResultReflections.IsUnwrappableType(type) || ResultReflections.IsScalarLike(ResultReflections.GetDataType(type)))
			return resultData;

		// A null value is written as null, never as an invented empty object.
		return resultData?.Data;
	}

	/// <summary>
	/// Returns whether a response body must be written for <paramref name="result"/>. A body may still be
	/// null (a successful typed result whose value is null), which is written as the JSON literal null.
	/// </summary>
	internal static bool HasBody(IResult? result)
	{
		if (result == null)
			return false;

		if (result.Problem != null)
			return true;

		return result.Status != 204 && result.GetType().IsGenericType;
	}
}
