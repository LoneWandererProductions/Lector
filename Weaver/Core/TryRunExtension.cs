/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        TryRunExtension.cs
 * PURPOSE:     Provides a preview of command execution with optional confirmation
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

            FeedbackRequest feedback = null!; // declare first

            feedback = new FeedbackRequest(
                prompt: $"Preview:\n{preview.Message}\nProceed with execution? (yes/no)",
                options: new[] { "yes", "no" },
                onRespond: input =>
                {
                    input = input.Trim().ToLowerInvariant();
                    return input switch
                    {
                        "yes" => executor(args),
                        "no" => CommandResult.Fail("Execution cancelled by user."),
                        _ => new CommandResult
                        {
                            Message = "Please answer yes/no",
                            RequiresConfirmation = true,
                            Feedback = feedback // now it works
                        }
                    };
                });


            return new CommandResult
            {
                Message = feedback.Prompt,
                Feedback = feedback,
                RequiresConfirmation = true
            };
        }
    }
}
