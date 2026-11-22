namespace Viana.Results.Tests;

// UnwrapResultSchemaTransformer (native OpenAPI) and UnwrapResultFilter (Swashbuckle) both decide
// with these helpers, so documentation follows the runtime body rule (FR-002, I8).
public class ResultReflectionsTests
{
    private readonly record struct PointStruct(int X, int Y);

    [Fact]
    public void CustomStruct_IsUnwrappedAndNotSimple()
    {
        var type = typeof(Result<PointStruct>);

        Assert.True(ResultReflections.IsUnwrappableType(type));
        Assert.False(ResultReflections.IsScalarLike(ResultReflections.GetDataType(type)));
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(int?))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(string))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(DateTimeOffset))]
    [InlineData(typeof(DayOfWeek))]
    public void SimpleValues_AreScalarLike(Type type)
    {
        Assert.True(ResultReflections.IsScalarLike(type));
    }
}
