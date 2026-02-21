using System.Net;

namespace Viana.Results.Mvc.Tests;

public class HttpStatusInfoTests
{
    public static IEnumerable<object[]> AllIntCodes => new[]
    {
        new object[] { 100, "Continue" },
        new object[] { 101, "Switching Protocols" },
        new object[] { 102, "Processing" },
        new object[] { 103, "Early Hints" },
        new object[] { 200, "OK" },
        new object[] { 201, "Created" },
        new object[] { 202, "Accepted" },
        new object[] { 203, "Non-Authoritative Information" },
        new object[] { 204, "No Content" },
        new object[] { 205, "Reset Content" },
        new object[] { 206, "Partial Content" },
        new object[] { 207, "Multi-Status" },
        new object[] { 208, "Already Reported" },
        new object[] { 226, "IM Used" },
        new object[] { 300, "Multiple Choices" },
        new object[] { 301, "Moved Permanently" },
        new object[] { 302, "Found" },
        new object[] { 303, "See Other" },
        new object[] { 304, "Not Modified" },
        new object[] { 305, "Use Proxy" },
        new object[] { 306, "Unused" },
        new object[] { 307, "Temporary Redirect" },
        new object[] { 308, "Permanent Redirect" },
        new object[] { 400, "Bad Request" },
        new object[] { 401, "Unauthorized" },
        new object[] { 402, "Payment Required" },
        new object[] { 403, "Forbidden" },
        new object[] { 404, "Not Found" },
        new object[] { 405, "Method Not Allowed" },
        new object[] { 406, "Not Acceptable" },
        new object[] { 407, "Proxy Authentication Required" },
        new object[] { 408, "Request Timeout" },
        new object[] { 409, "Conflict" },
        new object[] { 410, "Gone" },
        new object[] { 411, "Length Required" },
        new object[] { 412, "Precondition Failed" },
        new object[] { 413, "Content Too Large" },
        new object[] { 414, "URI Too Long" },
        new object[] { 415, "Unsupported Media Type" },
        new object[] { 416, "Range Not Satisfiable" },
        new object[] { 417, "Expectation Failed" },
        new object[] { 418, "(Unused)" },
        new object[] { 421, "Misdirected Request" },
        new object[] { 422, "Unprocessable Content" },
        new object[] { 423, "Locked" },
        new object[] { 424, "Failed Dependency" },
        new object[] { 425, "Too Early" },
        new object[] { 426, "Upgrade Required" },
        new object[] { 428, "Precondition Required" },
        new object[] { 429, "Too Many Requests" },
        new object[] { 431, "Request Header Fields Too Large" },
        new object[] { 451, "Unavailable For Legal Reasons" },
        new object[] { 500, "Internal Server Error" },
        new object[] { 501, "Not Implemented" },
        new object[] { 502, "Bad Gateway" },
        new object[] { 503, "Service Unavailable" },
        new object[] { 504, "Gateway Timeout" },
        new object[] { 505, "HTTP Version Not Supported" },
        new object[] { 506, "Variant Also Negotiates" },
        new object[] { 507, "Insufficient Storage" },
        new object[] { 508, "Loop Detected" },
        new object[] { 510, "Not Extended" },
        new object[] { 511, "Network Authentication Required" },
    };

    public static IEnumerable<object[]> AllHttpStatusCodes =>
        AllIntCodes.Select(x => new object[] { (HttpStatusCode)(int)x[0]!, x[1]! });

    [Theory]
    [MemberData(nameof(AllIntCodes))]
    public void GetMessage_WithIntCode_ReturnsCorrectMessage(int code, string expectedMessage)
    {
        var message = HttpStatusInfo.GetMessage(code);
        Assert.Equal(expectedMessage, message);
    }

    [Theory]
    [MemberData(nameof(AllHttpStatusCodes))]
    public void GetMessage_WithHttpStatusCode_ReturnsCorrectMessage(HttpStatusCode status, string expectedMessage)
    {
        var message = HttpStatusInfo.GetMessage(status);
        Assert.Equal(expectedMessage, message);
    }
}
