using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
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
    public void OnResultExecuting_WhenObjectResultWithIResult_ReplacesWithNativeResult()
    {
        var result = Results.Ok("data");
        var objectResult = new ObjectResult(result);
        var context = CreateExecutingContext(objectResult);
        var filter = new VianaResultFilter();

        filter.OnResultExecuting(context);

        var native = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(200, native.StatusCode);
        Assert.NotSame(objectResult, context.Result);
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

    [Fact]
    public void ToActionResult_WithProblem_ReturnsProblemDetailsAsProblemJson()
    {
        var problem = ProblemResult.WithDescription(422, "Validation Error", "Invalid input");
        var result = new Result(problem);

        var action = new VianaResultFilter().ToActionResult(result);

        var objectResult = Assert.IsType<ObjectResult>(action);
        Assert.Equal(422, objectResult.StatusCode);
        Assert.Contains("application/problem+json", objectResult.ContentTypes);
        Assert.DoesNotContain("application/problem+xml", objectResult.ContentTypes);
        var details = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(422, details.Status);
        Assert.Equal("Validation Error", details.Title);
        Assert.Equal("Invalid input", details.Detail);
    }

    [Fact]
    public void ToActionResult_WithProblem_UsesConfiguredProblemContentTypes()
    {
        var options = new VianaResultMvcOptions();
        options.ProblemContentTypes.Add("application/problem+xml");
        var filter = new VianaResultFilter(Options.Create(options));
        var result = new Result(new ProblemResult(400, "Bad Request"));

        var action = filter.ToActionResult(result);

        var objectResult = Assert.IsType<ObjectResult>(action);
        Assert.Contains("application/problem+json", objectResult.ContentTypes);
        Assert.Contains("application/problem+xml", objectResult.ContentTypes);
    }

    [Fact]
    public void ToActionResult_WithSuccessData_ReturnsObjectResultWithStatus()
    {
        var result = Results.Ok(new { Id = 1, Name = "Test" });

        var action = new VianaResultFilter().ToActionResult(result);

        var objectResult = Assert.IsType<ObjectResult>(action);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public void ToActionResult_WithNoBody_ReturnsBareStatusCode()
    {
        var result = Results.NoContent();

        var action = new VianaResultFilter().ToActionResult(result);

        var statusResult = Assert.IsType<StatusCodeResult>(action);
        Assert.Equal(204, statusResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_NonGenericSuccess_ReturnsBareStatusCode()
    {
        var result = Results.Ok();

        var action = new VianaResultFilter().ToActionResult(result);

        var statusResult = Assert.IsType<StatusCodeResult>(action);
        Assert.Equal(200, statusResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_ListResultSuccess_UnwrapsToArray()
    {
        var result = new ListResult<string>(new List<string> { "a", "b", "c" });

        var action = new VianaResultFilter().ToActionResult(result);

        var objectResult = Assert.IsType<ObjectResult>(action);
        Assert.Equal(200, objectResult.StatusCode);
        var list = Assert.IsAssignableFrom<IReadOnlyList<string>>(objectResult.Value);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void ToActionResult_PagedResultSuccess_KeepsPagedWrapper()
    {
        var result = PagedResult<int>.Create(new List<int> { 10, 20, 30 }, pageNumber: 2, pageSize: 3, totalCount: 9);

        var action = new VianaResultFilter().ToActionResult(result);

        var objectResult = Assert.IsType<ObjectResult>(action);
        var paged = Assert.IsType<PagedResult<int>>(objectResult.Value);
        Assert.Equal(2, paged.PageNumber);
        Assert.Equal(3, paged.TotalPages);
    }
}
