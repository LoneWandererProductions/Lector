/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        TryRunExtension.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <summary>
    /// Provides a preview of command execution and requests user confirmation.
    /// </summary>
    public sealed class TryRunExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => "tryrun";

        /// <inheritdoc />
        public string Description => "Provides a preview of command execution and requests user confirmation.";

        /// <inheritdoc />
        public CommandResult Invoke(ICommand command, string[] args, Func<string[], CommandResult> executor)
        {
            // Preview first
            var preview = command.TryRun(args) ?? executor(args);

            var feedback = new FeedbackRequest
            {
                Prompt = $"Preview:\n{preview.Message}\nProceed with execution? (yes/no)",
                Options = new[] { "yes", "no" }
            };

            return new CommandResult
            {
                Message = feedback.Prompt,
                Feedback = feedback,
                RequiresConfirmation = true
            };
        }
    }
}