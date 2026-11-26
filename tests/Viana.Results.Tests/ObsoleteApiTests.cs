using System.Reflection;

namespace Viana.Results.Tests;

public class ObsoleteApiTests
{
    [Fact]
    public void ProblemResult_WithDescription_IsObsoleteAndNamesTheReplacement()
    {
        var method = typeof(ProblemResult).GetMethod(nameof(ProblemResult.WithDescription), BindingFlags.Public | BindingFlags.Static);

        AssertObsolete(method, "ProblemResult.WithDetail");
    }

    [Fact]
    public void ProblemBuilder_WithDescription_IsObsoleteAndNamesTheReplacement()
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
        Assert.DoesNotMatch(@"\d+\.\d+\.\d+", obsolete.Message); // feature 015: no future version
        Assert.Contains(replacement, obsolete.Message);
    }
}
