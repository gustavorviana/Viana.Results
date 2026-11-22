namespace Viana.Results.Tests;

public class ResultResponseBodyTests
{
    private sealed class User
    {
        public string Name { get; set; } = "";
    }

    private readonly record struct PointStruct(int X, int Y);

    private enum Color { Red }

    [Fact]
    public void GetBody_SuccessWithNullValue_ReturnsNull()
    {
        var result = new Result<User>((User?)null);

        Assert.Null(ResultResponseBody.GetBody(result));
    }

    [Fact]
    public void GetBody_SuccessWithValue_ReturnsValue()
    {
        var user = new User { Name = "Ana" };

        Assert.Same(user, ResultResponseBody.GetBody(new Result<User>(user)));
    }

    [Fact]
    public void GetBody_FailureWithStatus204_ReturnsProblem()
    {
        var problem = new ProblemResult(400, "x");

        Assert.Same(problem, ResultResponseBody.GetBody(new Result(204, problem)));
    }

    [Fact]
    public void GetBody_Status204WithoutProblem_ReturnsNull()
    {
        Assert.Null(ResultResponseBody.GetBody(new Result(204)));
    }

    [Fact]
    public void GetBody_CustomStruct_IsUnwrapped()
    {
        var point = new PointStruct(1, 2);

        Assert.Equal(point, ResultResponseBody.GetBody(new Result<PointStruct>(point)));
    }

    public static TheoryData<IResult> SimpleValueResults() => new()
    {
        new Result<int>(1),
        new Result<DateTime>(DateTime.UnixEpoch),
        new Result<Guid>(Guid.Empty),
        new Result<string>("x"),
        new Result<Color>(Color.Red),
        new Result<int?>(5),
    };

    [Theory]
    [MemberData(nameof(SimpleValueResults))]
    public void GetBody_SimpleValues_KeepWrapper(IResult result)
    {
        Assert.Same(result, ResultResponseBody.GetBody(result));
    }

    [Fact]
    public void GetBody_ListResult_ReturnsItems()
    {
        var items = new List<int> { 1, 2 };

        var body = ResultResponseBody.GetBody(new ListResult<int>(items));

        Assert.Equal(items, Assert.IsAssignableFrom<IEnumerable<int>>(body));
    }

    [Fact]
    public void GetBody_PagedResult_KeepsWrapper()
    {
        var paged = new PagedResult<int>(new List<int> { 1 }, 1, 1);

        Assert.Same(paged, ResultResponseBody.GetBody(paged));
    }
}
