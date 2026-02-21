#if NET5_0_OR_GREATER
using System.Text.Json.Serialization;
#endif

namespace Viana.Results;

public class Result(int status, ProblemResult? problem = null) : IResult, IResultData
{
    private readonly object? _data;
    public int Status { get; } = status;
#if NET5_0_OR_GREATER
    [JsonIgnore]
#endif
    public ProblemResult? Problem { get; } = problem;

    object? IResultData.Data => _data;

    public Result(ProblemResult problem) : this(problem.Status, problem)
    {

    }

    internal Result(int status, object? data, ProblemResult? problem = null) : this(status, problem)
    {
        _data = data;
    }
}

public class Result<TValue>(TValue? data, int status = 200, ProblemResult? problem = null) : IResult<TValue>, IResultData
{
    public TValue? Data => data;

    object? IResultData.Data => Data;

    public int Status => status;

    public ProblemResult? Problem => problem;


    public Result(ProblemResult problem):this(default, problem.Status, problem)
    {
    }

    public static implicit operator Result<TValue>(TValue data) => new(data, 200);

    public static implicit operator Result<TValue>(ProblemResult problem) => new(default, problem.Status, problem);

    public static implicit operator Result(Result<TValue> result)
    {
        if (result.Problem != null)
            return new Result(result.Problem);
        return new Result(result.Status, result.Data, result.Problem);
    }

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