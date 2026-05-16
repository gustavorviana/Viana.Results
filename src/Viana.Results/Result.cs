using System.Text.Json.Serialization;

namespace Viana.Results;

/// <summary>
/// Non-generic result of an operation, carrying an HTTP-aligned <see cref="Status"/> and an optional
/// <see cref="Problem"/>. Use <see cref="Result{TValue}"/> when the operation also produces a payload.
/// </summary>
/// <param name="status">The HTTP status code that represents the outcome.</param>
/// <param name="problem">The problem details when the result represents a failure; <see langword="null"/> on success.</param>
public class Result(int status, ProblemResult? problem = null) : IResult, IResultData
{
    private readonly object? _data;

    /// <inheritdoc />
    [JsonIgnore]
    public int Status { get; } = status;

    /// <inheritdoc />
    [JsonIgnore]
    public ProblemResult? Problem { get; } = problem;

    object? IResultData.Data => _data;

    /// <summary>
    /// Initializes a failure result from a <see cref="ProblemResult"/>, taking its
    /// <see cref="ProblemResult.Status"/> as the result status.
    /// </summary>
    /// <param name="problem">The problem details describing the failure.</param>
    public Result(ProblemResult problem) : this(problem.Status, problem)
    {
    }

    internal Result(int status, object? data, ProblemResult? problem = null) : this(status, problem)
    {
        _data = data;
    }
}

/// <summary>
/// Result of an operation that produces a payload of type <typeparamref name="TValue"/>.
/// On failure, <see cref="Data"/> is the default value and <see cref="Problem"/> describes the error.
/// </summary>
/// <typeparam name="TValue">The payload type.</typeparam>
/// <param name="data">The payload value, or default when the result represents a failure.</param>
/// <param name="status">The HTTP status code. Defaults to 200 (OK).</param>
/// <param name="problem">The problem details when the result represents a failure; <see langword="null"/> on success.</param>
public class Result<TValue>(TValue? data, int status = 200, ProblemResult? problem = null) : IResult<TValue>, IResultData
{
    /// <inheritdoc />
    public TValue? Data => data;

    object? IResultData.Data => Data;

    /// <inheritdoc />
    [JsonIgnore]
    public int Status => status;

    /// <inheritdoc />
    [JsonIgnore]
    public ProblemResult? Problem => problem;


    /// <summary>
    /// Initializes a failure result from a <see cref="ProblemResult"/>, with <see cref="Data"/> set
    /// to the default value of <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="problem">The problem details describing the failure.</param>
    public Result(ProblemResult problem) : this(default, problem.Status, problem)
    {
    }

    /// <summary>
    /// Implicitly converts a payload value into a successful <see cref="Result{TValue}"/> with status 200.
    /// </summary>
    /// <param name="data">The payload value.</param>
    public static implicit operator Result<TValue>(TValue data) => new(data, 200);

    /// <summary>
    /// Implicitly converts a <see cref="ProblemResult"/> into a failure <see cref="Result{TValue}"/>
    /// that adopts the problem's status code.
    /// </summary>
    /// <param name="problem">The problem details describing the failure.</param>
    public static implicit operator Result<TValue>(ProblemResult problem) => new(default, problem.Status, problem);

    /// <summary>
    /// Implicitly converts a typed <see cref="Result{TValue}"/> into a non-generic <see cref="Result"/>,
    /// preserving status, payload and any problem details.
    /// </summary>
    /// <param name="result">The typed result to convert.</param>
    public static implicit operator Result(Result<TValue> result)
    {
        if (result.Problem != null)
            return new Result(result.Problem);
        return new Result(result.Status, result.Data, result.Problem);
    }

    /// <summary>
    /// Implicitly converts a non-generic <see cref="Result"/> into a typed <see cref="Result{TValue}"/>.
    /// The payload is preserved when it can be cast to <typeparamref name="TValue"/>; otherwise it is the default value.
    /// </summary>
    /// <param name="result">The non-generic result to convert.</param>
    public static implicit operator Result<TValue>(Result result)
    {
        if (result.Problem != null)
            return new Result<TValue>(default, result.Status, result.Problem);

        TValue? typedData = default;
        if (((IResultData)result)?.Data is TValue value)
            typedData = value;

        return new Result<TValue>(typedData, result!.Status);
    }
}
