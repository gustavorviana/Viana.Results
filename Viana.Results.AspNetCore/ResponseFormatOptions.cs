namespace Viana.Results.AspNetCore
{
    /// <summary>
    /// Defines configuration options for how API responses are formatted.
    /// This setting controls whether response objects are wrapped in an envelope object.
    /// </summary>
    public class ResponseFormatOptions
    {
        /// <summary>
        /// Determines whether API responses should be wrapped in an envelope object.
        /// <para>
        /// When set to <c>true</c>, responses are returned in the format <c>{ data: ... }</c>.
        /// </para>
        /// <para>
        /// When set to <c>false</c>, the response value is returned directly (without the <c>data</c> wrapper).
        /// </para>
        /// </summary>
        public bool UseObjectEnvelope { get; set; } = true;
    }
}
