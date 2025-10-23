/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        CommandResult.cs
 * PURPOSE:     Message Object, encapsulates the result of a command execution.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Messages
{
    /// <summary>
    /// The Result of a command execution.
    /// </summary>
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
        public bool Success { get; set; }

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

        public static CommandResult Fail(string msg) => new() { Success = false, Message = msg };

        public FeedbackRequest? Feedback { get; init; }
    }
}