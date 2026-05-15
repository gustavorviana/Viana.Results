namespace Viana.Results.OpenApi;

/// <summary>
/// Provides an example payload to be embedded in the generated OpenAPI document
/// for a given response status code. Implementations are resolved via DI when registered;
/// otherwise they are instantiated via <see cref="System.Activator"/>.
/// </summary>
/// <typeparam name="T">
/// The example payload type. For success responses this is typically the action's return DTO.
/// For error responses this is usually <see cref="ProblemResult"/>.
/// </typeparam>
public interface IExampleProvider<out T>
{
    /// <summary>Returns the example payload.</summary>
    T GetExample();
}
