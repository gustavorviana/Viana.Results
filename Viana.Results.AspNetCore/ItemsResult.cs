using System.Collections.Generic;
using System.Net;

namespace Viana.Results.AspNetCore
{
    public class ItemsResult : ActionResultBase
    {
        public ICollection<object> Items { get; set; } = [];
        public string Message { get; set; } = string.Empty;

        public ItemsResult() : base(HttpStatusCode.OK)
        {

        }

        public ItemsResult(HttpStatusCode status) : base(status)
        {
        }

        protected override object GetReturnObject()
        {
            Items ??= [];

            if (string.IsNullOrEmpty(Message))
                return new { Items };

            return new { Message, Items };
        }
    }
}
