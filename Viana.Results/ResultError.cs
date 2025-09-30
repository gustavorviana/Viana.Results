namespace Viana.Results
{
    public class DataResultError : ResultError
    {
        public object Data { get; private set; }

        public DataResultError(object data, string message = null) : base(message ?? "Ocorreu um erro")
        {
            Data = data;
        }
    }

    public class ResultError
    {
        public string Message { get; private set; }

        public ResultError(string message)
        {
            Message = message;
        }
    }
}
