namespace Viana.Results.Tests;

public class HttpStatusInfoTests
{
    [Theory]
    [InlineData(404, "Not Found")]
    [InlineData(500, "Internal Server Error")]
    [InlineData(409, "Conflict")]
    public void TryGetReasonPhrase_KnownStatus_ReturnsPhrase(int status, string expected)
    {
        Assert.True(HttpStatusInfo.TryGetReasonPhrase(status, out var phrase));
        Assert.Equal(expected, phrase);
    }

    [Theory]
    [InlineData(306)]
    [InlineData(418)]
    [InlineData(499)]
    [InlineData(299)]
    [InlineData(0)]
    [InlineData(-1)]
    public void TryGetReasonPhrase_PlaceholderOrUnknownStatus_ReturnsFalse(int status)
    {
        Assert.False(HttpStatusInfo.TryGetReasonPhrase(status, out _));
    }

    [Fact]
    public void GetMessage_UnknownStatus_StillReturnsUnknown()
    {
        Assert.Equal("Unknown", HttpStatusInfo.GetMessage(499));
    }
}
