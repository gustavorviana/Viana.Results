using System.Dynamic;
using System.Net;

namespace Viana.Results.AspNetCore
{
    public class ObjectResult : ActionResultBase
    {
        public object Result { get; set; }
        public string Message { get; set; } = string.Empty;

        public ObjectResult() : base(HttpStatusCode.OK)
        {

        }

        public ObjectResult(HttpStatusCode status) : base(status)
        {
        }

        public static ObjectResult ForError(HttpStatusCode status, object obj)
        {
            return new ObjectResult(status)
            {
                Result = obj,
                Error = new DataResultError(obj)
            };
        }

        protected override object GetReturnObject()
        {
            dynamic obj = new ExpandoObject();
            if (Error != null)
                obj.Error = Error;

            obj.Result = Result;

            if (!string.IsNullOrEmpty(Message))
                obj.Message = Message;

            return obj;
        }
    }
}
