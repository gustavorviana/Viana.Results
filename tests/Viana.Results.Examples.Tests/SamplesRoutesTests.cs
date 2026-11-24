using System.Net;

namespace Viana.Results.Examples.Tests;

/// <summary>Every <c>/api/samples</c> route answers the status of the factory it shows.</summary>
public class SamplesRoutesTests
{
    public static TheoryData<string, string, string, int> Routes()
    {
        var routes = new (string Method, string Path, int Status)[]
        {
            ("GET", "ok", 200), ("GET", "ok-message", 200), ("GET", "ok-value", 200), ("GET", "implicit", 200),
            ("POST", "created", 201), ("DELETE", "no-content", 204), ("GET", "list", 200), ("GET", "paged", 200),
            ("GET", "bad-request", 400), ("GET", "validation", 400), ("GET", "unauthorized", 401),
            ("GET", "forbidden", 403), ("GET", "not-found", 404), ("GET", "conflict", 409),
            ("GET", "business-rule", 422), ("GET", "failure", 503), ("GET", "failure-exception", 500),
            ("GET", "builder", 409), ("GET", "problem-result", 402), ("GET", "crash", 500)
        };

        var data = new TheoryData<string, string, string, int>();
        foreach (var host in new[] { Hosts.Scalar, Hosts.Swagger })
            foreach (var (method, path, status) in routes)
                data.Add(host, method, path, status);
        return data;
    }

    [Theory, MemberData(nameof(Routes))]
    public async Task Sample_returns_its_status(string host, string method, string path, int status)
    {
        var response = await Hosts.Create(host).SendAsync(new HttpRequestMessage(new HttpMethod(method), $"/api/samples/{path}"));

        Assert.Equal((HttpStatusCode)status, response.StatusCode);
        if (status >= 400)
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData(Hosts.Scalar)]
    [InlineData(Hosts.Swagger)]
    public async Task Builder_sample_carries_every_member(string host)
    {
        var body = await (await Hosts.Create(host).GetAsync("/api/samples/builder")).Json();

        Assert.Equal("https://api.example.com/errors/reservation-expired", body.GetProperty("type").GetString());
        Assert.Equal("The reservation 77 expired at 10:00.", body.GetProperty("detail").GetString());
        Assert.Equal("/api/reservations/77", body.GetProperty("instance").GetString());
        Assert.Equal(77, body.GetProperty("reservationId").GetInt32());
        Assert.True(body.TryGetProperty("expiredAt", out _));
    }
}
