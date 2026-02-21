using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Viana.Results.Mvc.Filters;

namespace Viana.Results.Mvc.Tests;

public class VianaResultFilterTests
{
    private static ResultExecutingContext CreateExecutingContext(IActionResult result)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());
    }

    [Fact]
    public void OnResultExecuting_WhenObjectResultWithIResult_ReplacesWithVianaResultAction()
    {
        var result = Results.Ok("data");
        var objectResult = new ObjectResult(result);
        var context = CreateExecutingContext(objectResult);
        var filter = new VianaResultFilter();

        filter.OnResultExecuting(context);

        Assert.IsType<global::Viana.Results.Mvc.VianaResultAction>(context.Result);
    }

    [Fact]
    public void OnResultExecuting_WhenObjectResultWithNonIResult_DoesNotReplace()
    {
        var objectResult = new ObjectResult(new { Foo = "bar" });
        var context = CreateExecutingContext(objectResult);
        var filter = new VianaResultFilter();

        filter.OnResultExecuting(context);

        Assert.Same(objectResult, context.Result);
    }

    [Fact]
    public void OnResultExecuting_WhenResultIsNotObjectResult_DoesNotReplace()
    {
        var viewResult = new ViewResult();
        var context = CreateExecutingContext(viewResult);
        var filter = new VianaResultFilter();

        filter.OnResultExecuting(context);

        Assert.Same(viewResult, context.Result);
    }

    [Fact]
    public void OnResultExecuting_WhenObjectResultValueIsNull_DoesNotReplace()
    {
        var objectResult = new ObjectResult(null);
        var context = CreateExecutingContext(objectResult);
        var filter = new VianaResultFilter();

        filter.OnResultExecuting(context);

        Assert.Same(objectResult, context.Result);
    }

    [Fact]
    public void OnResultExecuted_DoesNotThrow()
    {
        var context = new ResultExecutedContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new ObjectResult(Results.Ok()),
            new object());
        var filter = new VianaResultFilter();

        filter.OnResultExecuted(context);
    }
}
