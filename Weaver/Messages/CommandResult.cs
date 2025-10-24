/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        CommandResult.cs
 * PURPOSE:     Message Object, encapsulates the result of a command execution.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;

namespace Weaver.Messages
{
    /// <summary>
    /// The Result of a command execution.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public sealed class CommandResult
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; init; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CommandResult"/> is success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success { get; init; }

        /// <summary>
        /// Gets a value indicating whether [requires confirmation].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires confirmation]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresConfirmation { get; init; }

        /// <summary>
        /// Gets the suggestions.
        /// </summary>
        /// <value>
        /// The suggestions.
        /// </value>
        public string[]? Suggestions { get; init; }

        /// <summary>
        /// Oks the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        public static CommandResult Ok(string msg) => new() { Success = true, Message = msg };

        /// <summary>
        /// Fails the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        public static CommandResult Fail(string msg) => new() { Success = false, Message = msg };

        /// <summary>
        /// Gets the feedback.
        /// </summary>
        /// <value>
        /// The feedback.
        /// </value>
        public FeedbackRequest? Feedback { get; init; }

        /// <summary>
        /// Returns a human-readable string representation of the command result.
        /// </summary>
        public override string ToString()
        {
            var suggestionsPart = Suggestions == null || Suggestions.Length == 0
                ? "<none>"
                : string.Join(", ", Suggestions);

            var feedbackPart = Feedback == null ? "<none>" : $"FeedbackId={Feedback.RequestId}";

            return $"Success={Success}, RequiresConfirmation={RequiresConfirmation}, Message=\"{Message}\", Suggestions=[{suggestionsPart}], {feedbackPart}";
        }
    }
}