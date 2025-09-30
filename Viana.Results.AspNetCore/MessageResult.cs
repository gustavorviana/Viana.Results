using System.Dynamic;
using System.Net;

namespace Viana.Results.AspNetCore
{
    public class MessageResult : ActionResultBase
    {
        public string Message { get; set; } = string.Empty;

        public MessageResult() : base(HttpStatusCode.OK)
        {
        }

        public MessageResult(HttpStatusCode status) : base(status)
        {
        }

        public static MessageResult Ok(string message = "OK")
        {
            return new MessageResult { Message = message };
        }

        protected override object GetReturnObject()
        {
            dynamic obj = new ExpandoObject();
            if (Error != null)
                obj.Error = Error;

            if (!string.IsNullOrEmpty(Message))
                obj.Message = Message;

            return obj;
        }

        public static MessageResult ServiceUnavailable(int retryAfter = 120)
        {
            MessageResult result = new(HttpStatusCode.ServiceUnavailable);

            result.Headers.Add("Retry-After", retryAfter.ToString());
            return result;
        }
    }
}
