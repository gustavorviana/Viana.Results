using System.Reflection;

namespace Viana.Results.Tests;

public class ObsoleteApiTests
{
    [Fact]
    public void ProblemResult_WithDescription_IsObsoleteWithRemovalNotice()
    {
        var method = typeof(ProblemResult).GetMethod(nameof(ProblemResult.WithDescription), BindingFlags.Public | BindingFlags.Static);

        AssertObsolete(method, "ProblemResult.WithDetail");
    }

    [Fact]
    public void ProblemBuilder_WithDescription_IsObsoleteWithRemovalNotice()
    {
        var method = typeof(ProblemBuilder).GetMethod(nameof(ProblemBuilder.WithDescription), BindingFlags.Public | BindingFlags.Instance);

        AssertObsolete(method, "ProblemBuilder.WithDetail");
    }

    private static void AssertObsolete(MethodInfo? method, string replacement)
    {
        Assert.NotNull(method);
        var obsolete = method.GetCustomAttribute<ObsoleteAttribute>();
        Assert.NotNull(obsolete);
        Assert.False(obsolete.IsError);
        Assert.Contains("RFC 9457", obsolete.Message);
        Assert.Contains("4.0.0", obsolete.Message);
        Assert.Contains(replacement, obsolete.Message);
    }
}
