/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        AppendExtension.cs
 * PURPOSE:     Test Extenion that appends "[EXT]" to the command message
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Mediator
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
        public CommandResult Invoke(ICommand command, string[] args, Func<string[], CommandResult> executor)
        {
            // Execute the command normally
            var result = executor(args);

            // Append a note
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