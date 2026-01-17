/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Core
 * FILE:        AppendExtension.cs
 * PURPOSE:     Test Extenion that appends "[EXT]" to the command message
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Mediator.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Test extension that appends "[EXT]" to the command message
    /// </summary>
    public sealed class AppendExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => "append";

        /// <inheritdoc />
        public string Description => "Test";

        /// <inheritdoc />
        public string Namespace => "Test";

        /// <inheritdoc />
        public CommandResult Invoke(ICommand command, string[] extensionArgs, Func<string[], CommandResult> executor,
            string[] commandArgs)
        {
            // Execute the original command with proper command args
            var result = executor(commandArgs);

            // Now handle extension args as needed (e.g., store key, append message, etc.)
            // Here we just append "[EXT]" for demo
            return new CommandResult
            {
                Message = $"{result.Message} [EXT]",
                Success = result.Success,
                Feedback = result.Feedback,
                RequiresConfirmation = result.RequiresConfirmation
            };
        }
    }
}