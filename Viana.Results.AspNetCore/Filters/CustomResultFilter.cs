using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IO;
using System.Net;

namespace Viana.Results.AspNetCore.Filters
{
    /// <summary>
    /// Action filter that transforms action results into custom result types for consistent API responses.
    /// </summary>
    public class CustomResultFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Called after the action method is executed.
        /// Converts the result to a custom result type if applicable.
        /// </summary>
        /// <param name="context">The context for the action executed.</param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
            {
                var result = GetResult(objectResult);
                if (result != null)
                    context.Result = result;
            }
            else if (context.Result is StatusCodeResult statusCodeResult)
            {
                context.Result = new MessageResult((HttpStatusCode)statusCodeResult.StatusCode);
            }

            base.OnActionExecuted(context);
        }

        private static ActionResultBase GetResult(Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
        {
            if (objectResult.Value == null)
                return null;

            if (objectResult.Value is IResult result)
                return ActionResultBase.FromResult(result);

            if (objectResult.Value is string strResult)
                return new MessageResult
                {
                    Message = strResult
                };

            var valueType = objectResult.Value.GetType();
            if (typeof(Stream).IsAssignableFrom(valueType) || valueType == typeof(byte[]))
                return null;

            return ActionResultBase.FromResult(new Result(string.Empty, objectResult.Value));
        }
    }
}
