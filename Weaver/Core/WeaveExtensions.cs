/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        WeaveExtensions.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <summary>
    /// Handles built-in universal extensions like .help() and .tryrun().
    /// </summary>
    public static class WeaveExtensions
    {
        public static bool TryHandleBuiltinExtension(
            ICommand command,
            string extensionName,
            string[] args,
            out CommandResult result)
        {
            result = default!;

            // Universal .help() — just show the command description
            if (string.Equals(extensionName, "help", StringComparison.OrdinalIgnoreCase))
            {
                result = CommandResult.Ok(command.Description);
                return true;
            }

            // Universal .tryrun() — preview result and ask user for confirmation
            if (string.Equals(extensionName, "tryrun", StringComparison.OrdinalIgnoreCase))
            {
                var preview = command.Execute(args);

                result = new CommandResult
                {
                    Message = $"Preview:\n{preview.Message}\n\nProceed with actual execution?",
                    Feedback = new FeedbackRequest
                    {
                        Prompt = "Proceed? (yes/no)",
                        Options = new[] { "yes", "no" }
                    }
                };
                return true;
            }

            return false; // let command handle its own extensions
        }

        /// <summary>
        /// Resumes after .tryrun() feedback input (yes/no).
        /// </summary>
        public static CommandResult HandleTryRunFeedback(ICommand command, string feedbackInput, string[] args)
        {
            var normalized = feedbackInput.Trim().ToLowerInvariant();

            if (normalized == "yes")
            {
                var actual = command.Execute(args);
                return CommandResult.Ok($"Executed successfully: {actual.Message}");
            }

            if (normalized == "no")
                return CommandResult.Fail("Execution cancelled by user.");

            return new CommandResult
            {
                Message = "Please answer yes or no.",
                Feedback = new FeedbackRequest
                {
                    Prompt = "Proceed? (yes/no)",
                    Options = new[] { "yes", "no" }
                }
            };
        }
    }
}
