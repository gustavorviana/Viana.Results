using System.Collections.Generic;

namespace Viana.Results
{
    /// <summary>
    /// Represents a validation error with field-level error messages.
    /// </summary>
    public class ValidationError : ResultError
    {
        /// <summary>
        /// Gets the validation errors grouped by field name.
        /// </summary>
        public Dictionary<string, string[]> Errors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="errors">The validation errors grouped by field name.</param>
        public ValidationError(Dictionary<string, string[]> errors, string message = "Validation failed") : base(message)
        {
            Errors = errors ?? [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class with a single field error.
        /// </summary>
        /// <param name="field">The field name.</param>
        /// <param name="messages">The error messages for the field.</param>
        public ValidationError(string field, params string[] messages) : base("Validation failed")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, messages ?? [] }
            };
        }
    }

    /// <summary>
    /// Represents an error with associated data.
    /// </summary>
    public class DataResultError : ResultError
    {
        /// <summary>
        /// Gets the data associated with the error.
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataResultError"/> class.
        /// </summary>
        /// <param name="data">The data associated with the error.</param>
        /// <param name="message">The error message. Defaults to "Ocorreu um erro" if not provided.</param>
        public DataResultError(object data, string message = null) : base(message ?? "Ocorreu um erro")
        {
            Data = data;
        }
    }

    /// <summary>
    /// Represents a basic error with message.
    /// </summary>
    public class ResultError
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultError"/> class.
        /// </summary>
        /// <param name="message">The error messages.</param>
        public ResultError(string message)
        {
            Message = message;
        }
    }
}
