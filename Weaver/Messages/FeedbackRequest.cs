/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        FeedbackRequest.cs
 * PURPOSE:     Description of a feedback request message.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;

namespace Weaver.Messages
{
    /// <summary>
    ///     Description of a feedback request message.
    /// </summary>
    public sealed class FeedbackRequest : IFeedback
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
        public string Prompt { get; init; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public string[] Options { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Indicates if feedback is still pending.
        /// </summary>
        public bool IsPending { get; private set; } = true;

        /// <summary>
        /// The on respond
        /// </summary>
        private readonly Func<string, CommandResult> _onRespond;

        /// <summary>
        /// Possible valid options.
        /// </summary>
        IReadOnlyList<string> IFeedback.Options => Options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackRequest"/> class.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="options">The options.</param>
        /// <param name="onRespond">The on respond.</param>
        public FeedbackRequest(string prompt, string[] options, Func<string, CommandResult> onRespond)
        {
            Prompt = prompt;
            Options = options;
            _onRespond = onRespond;
        }

        /// <summary>
        /// Handles a user response and returns a CommandResult.
        /// </summary>
        /// <param name="input">User input</param>
        /// <returns>Result of Input.</returns>
        public CommandResult Respond(string input)
        {
            var result = _onRespond(input);
            if (!result.RequiresConfirmation)
                IsPending = false;

            return result;
        }
    }
}