/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        FeedbackRequest.cs
 * PURPOSE:     Description of a feedback request message.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Messages
{
    /// <summary>
    ///     Description of a feedback request message.
    /// </summary>
    public sealed class FeedbackRequest
    {
        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public string RequestId { get; init; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the prompt.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        public string Prompt { get; init; } = "";

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public string[] Options { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets a value indicating whether [expect exact match].
        /// optional hint for frontend
        /// </summary>
        /// <value>
        ///   <c>true</c> if [expect exact match]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpectExactMatch { get; init; } = true;
    }
}