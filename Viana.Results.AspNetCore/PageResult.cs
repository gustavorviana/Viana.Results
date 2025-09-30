using System.Dynamic;
using System.Net;

namespace Viana.Results.AspNetCore
{
    public class PageResult : ItemsResult
    {
        public long TotalItems { get; set; }
        public int Pages { get; set; }

        public PageResult() : base(HttpStatusCode.OK)
        {
        }

        protected override object GetReturnObject()
        {
            dynamic obj = new ExpandoObject();

            obj.TotalItems = TotalItems;
            obj.Pages = Pages;

            if (!string.IsNullOrEmpty(Message))
                obj.Message = Message;

            if (Error != null)
                obj.Error = Error;

            obj.Items = Items ?? [];

            return obj;
        }
    }
}
